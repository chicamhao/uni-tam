/**
 * phalanx extension — implements the phalanx multi-agent architecture for pi.
 *
 * Read order mirrors `phalanx-architecture.yaml`: roles -> rules -> extend.
 *
 * Registered surface:
 *   tools:
 *     agora            shared memory + message bus (single_state)
 *     phalanx_dispatch dispatch a role with chain-of-command enforcement,
 *                      shield_wall retry, and consult_the_oracle escalation
 *     phalanx_status   inspect roles, rules, agents, and agora state
 *   commands:
 *     /phalanx                 summary
 *     /phalanx add-lochos      extend: add a coordinator domain
 *     /phalanx add-hoplite     extend: add a specialist (one tool)
 *     /phalanx reset           clear agora
 */

import * as fs from "node:fs";
import * as path from "node:path";
import { StringEnum } from "@earendil-works/pi-ai";
import type { ExtensionAPI, ExtensionContext } from "@earendil-works/pi-coding-agent";
import { Type } from "typebox";

import {
  appendHoplite,
  appendLochagosInstance,
  isHopliteAgent,
  isLochagosAgent,
  loadArchitecture,
  mayDispatch,
  resolveRole,
  type PhalanxArchitecture,
} from "./architecture.ts";
import { AgoraStore } from "./agora.ts";
import { discoverAgents, findAgent, formatAgentList, type AgentConfig } from "./agents.ts";
import { runSubagent, type DispatchResult } from "./dispatch.ts";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function text(s: string) {
  return { content: [{ type: "text", text: s }], details: {} };
}

function parseValue(s: string): unknown {
  const t = s.trim();
  if (t === "") return null;
  try {
    return JSON.parse(t);
  } catch {
    return s;
  }
}

function truncate(s: string, max = 2000): string {
  if (s.length <= max) return s;
  return s.slice(0, max) + `… (truncated ${s.length - max} chars)`;
}

function listValidRoles(arch: PhalanxArchitecture): string {
  const parts = ["psiloi"];
  for (const inst of arch.roles.lochagos?.instances ?? []) parts.push(`lochagos-${inst}`);
  for (const d of arch.roles.hoplites?.direct_reports ?? []) parts.push(`hoplite-${d.id}`);
  return parts.join(", ");
}

function chainOfCommandError(arch: PhalanxArchitecture, agentName: string): string {
  const domains = (arch.roles.lochagos?.instances ?? []).join("/");
  const direct = (arch.roles.hoplites?.direct_reports ?? []).map((d) => d.id).join("/");
  return (
    `chain_of_command: strategos may not dispatch "${agentName}" directly. ` +
    `strategos dispatches psiloi, lochagos (${domains}), and direct-report hoplites (${direct}). ` +
    `Other hoplites are dispatched by their owning lochagos.`
  );
}

function buildAgoraContext(agora: AgoraStore, role: string): string {
  const snap = agora.snapshot();
  const keys = Object.keys(snap.keys);
  const inbox = snap.messages.filter((m) => m.to === role || m.to === "*").slice(-5);
  const parts: string[] = [];
  if (keys.length) parts.push("keys: " + truncate(JSON.stringify(snap.keys), 3000));
  if (inbox.length) parts.push("inbox: " + JSON.stringify(inbox.map((m) => ({ from: m.from, content: m.content }))));
  if (parts.length === 0) return "";
  return "[agora context — shared state]\n" + parts.join("\n");
}

function buildRulesFragment(arch: PhalanxArchitecture): string {
  const lines = ["# Phalanx operating rules (enforced)", ""];
  for (const r of arch.rules) {
    lines.push(`- ${r.id}: ${r.statement}`);
  }
  return lines.join("\n");
}

function buildRosterFragment(arch: PhalanxArchitecture, agents: AgentConfig[]): string {
  const lines = ["# Phalanx roster (dispatchable roles)", ""];
  lines.push("- psiloi (scout): fast, cheap recon — dispatch first (scout_first)");
  for (const inst of arch.roles.lochagos?.instances ?? []) {
    lines.push(`- lochagos-${inst} (coordinator): breaks the objective into tasks for the ${inst} domain`);
  }
  for (const d of arch.roles.hoplites?.direct_reports ?? []) {
    lines.push(`- hoplite-${d.id} (specialist, direct report): ${d.responsibility ?? "single-task specialist"}`);
  }
  lines.push("");
  lines.push("Dispatch with the phalanx_dispatch tool. Keep all shared state in agora (single_state).");
  lines.push("On failure, retry at the narrowest scope once, then escalate (shield_wall).");
  lines.push("If ambiguous or retries exhausted, ask the user (consult_the_oracle).");
  const available = agents.map((a) => a.name).join(", ");
  lines.push(`Loaded agents: ${available || "none"}`);
  return lines.join("\n");
}

// ---------------------------------------------------------------------------
// Agent file templates (for extend commands)
// ---------------------------------------------------------------------------

function lochagosTemplate(name: string): string {
  return `---
name: lochagos-${name}
description: Coordinator for the ${name} domain — breaks objectives into hoplite tasks
tools: read, grep, find, ls, bash
---

You are a **lochagos** (coordinator) for the "${name}" domain. You report to the
strategos. Your job is to break one objective into concrete, executable tasks for
the ${name} domain and carry them out with the tools you have.

Rules:
- chain_of_command: escalate failure up to the strategos, never sideways.
- shield_wall: on failure, retry at the narrowest scope once, then escalate.
- single_state: do not keep private state; read and write shared state via agora.
`;
}

function hopliteTemplate(id: string, lochagos: string, tool: string): string {
  return `---
name: hoplite-${id}
description: Specialist hoplite "${id}" (reports to ${lochagos}) — exactly one task, one tool
tools: ${tool}
---

You are a **hoplite** (specialist) named "${id}". You report to the lochagos
"${lochagos}". You execute exactly one task using exactly one tool (${tool}).

Rules:
- Do exactly one task; a task needing two tools becomes two hoplites.
- chain_of_command: escalate failure to your lochagos (${lochagos}), never sideways.
- shield_wall: retry once at the narrowest scope, then escalate.
- single_state: no private state; use agora for shared memory.
`;
}

function ensureAgentsDir(cwd: string): string {
  const dir = path.join(cwd, ".pi", "agents");
  fs.mkdirSync(dir, { recursive: true });
  return dir;
}

// ---------------------------------------------------------------------------
// Extension
// ---------------------------------------------------------------------------

export default function (pi: ExtensionAPI) {
  let agora: AgoraStore | null = null;
  const getAgora = (cwd: string): AgoraStore => {
    if (!agora) agora = new AgoraStore(cwd);
    return agora;
  };

  // ---- roles/agents discovered fresh on each use (picks up extend edits) ----

  // -------------------------------------------------------------------------
  // agora tool
  // -------------------------------------------------------------------------
  pi.registerTool({
    name: "agora",
    label: "Agora",
    description:
      "Phalanx shared memory and message bus (the single source of state). " +
      "Actions: get, put (value is a JSON string), del, list, post (from/to/content), " +
      "inbox (to), mark_read (id), log (event/from/content), attempts (scope).",
    promptSnippet: "Read/write phalanx shared memory (agora) and the message bus",
    promptGuidelines: [
      "Use agora for ALL shared state (single_state rule) — never keep private state in the conversation.",
    ],
    parameters: Type.Object({
      action: StringEnum(
        ["get", "put", "del", "list", "post", "inbox", "mark_read", "log", "attempts"] as const,
        { description: "Which agora operation to perform" },
      ),
      key: Type.Optional(Type.String({ description: "Key for get/put/del" })),
      value: Type.Optional(Type.String({ description: "Value for put, as a JSON string" })),
      from: Type.Optional(Type.String({ description: "Sender role for post/log (default strategos)" })),
      to: Type.Optional(Type.String({ description: "Recipient role for post/inbox ('*' = broadcast)" })),
      content: Type.Optional(Type.String({ description: "Content for post/log" })),
      id: Type.Optional(Type.String({ description: "Message id for mark_read" })),
      scope: Type.Optional(Type.String({ description: "Retry scope for attempts" })),
      event: Type.Optional(Type.String({ description: "Event name for log" })),
    }),

    async execute(_toolCallId, params, _signal, _onUpdate, ctx) {
      const agora = getAgora(ctx.cwd);
      switch (params.action) {
        case "get":
          return text(`${agora.get(params.key ?? "") ?? "(not set)"}`);
        case "put": {
          const v = parseValue(params.value ?? "");
          await agora.put(params.key ?? "", v);
          return text(`agora.put("${params.key}") = ${JSON.stringify(v)}`);
        }
        case "del": {
          const had = await agora.del(params.key ?? "");
          return text(had ? `deleted "${params.key}"` : `no such key: "${params.key}"`);
        }
        case "list":
          return text(agora.listKeys().join("\n") || "(empty)");
        case "post": {
          const m = await agora.post(params.from ?? "strategos", params.to ?? "*", params.content ?? "");
          return text(`posted ${m.id} from ${m.from} -> ${m.to}`);
        }
        case "inbox": {
          const msgs = agora.inbox(params.to ?? "strategos");
          return text(
            msgs.length
              ? msgs
                  .map((m) => `${m.id} [${m.from}->${m.to}]${m.read ? "" : " (unread)"} ${m.content}`)
                  .join("\n")
              : "(empty)",
          );
        }
        case "mark_read": {
          const ok = await agora.markRead(params.id ?? "");
          return text(ok ? "marked read" : "no such message id");
        }
        case "log": {
          await agora.log(params.event ?? "note", params.from ?? "strategos", params.content);
          return text("logged");
        }
        case "attempts":
          return text(String(agora.attemptsFor(params.scope ?? "")));
        default:
          return text("unknown action");
      }
    },
  });

  // -------------------------------------------------------------------------
  // phalanx_dispatch tool
  // -------------------------------------------------------------------------
  pi.registerTool({
    name: "phalanx_dispatch",
    label: "Phalanx Dispatch",
    description:
      "Dispatch a phalanx role to execute one task in an isolated context. " +
      "Roles: psiloi (scout), lochagos-<domain> (coordinator), hoplite-<id> (specialist, direct report). " +
      "Enforces chain_of_command, shield_wall (retry once then escalate), and consult_the_oracle.",
    promptSnippet: "Dispatch a phalanx role (psiloi, lochagos-<domain>, hoplite-<id>) for one task",
    promptGuidelines: [
      "Use phalanx_dispatch to delegate work: dispatch psiloi first (scout_first), then lochagos/hoplites. Never dispatch sideways (chain_of_command).",
    ],
    parameters: Type.Object({
      role: Type.String({
        description: "Role to dispatch: psiloi, lochagos-<domain>, or hoplite-<id> (a direct report)",
      }),
      task: Type.String({ description: "The single task for the role to execute" }),
      tool: Type.Optional(
        Type.String({ description: "For hoplites: the single tool to allow (exactly one tool per hoplite)" }),
      ),
      maxAttempts: Type.Optional(
        Type.Number({ description: "Maximum attempts before escalation (default 2)", default: 2 }),
      ),
      askOracleOnExhaust: Type.Optional(
        Type.Boolean({ description: "Ask the user when retries are exhausted (default true)", default: true }),
      ),
    }),

    async execute(_toolCallId, params, signal, onUpdate, ctx) {
      const arch = loadArchitecture(ctx.cwd);
      const agents = discoverAgents(ctx.cwd);
      const agora = getAgora(ctx.cwd);

      const roleName = params.role.trim();
      const resolved = resolveRole(arch, roleName);
      if (!resolved) {
        return {
          content: [{ type: "text", text: `Unknown role "${roleName}". Valid roles: ${listValidRoles(arch)}` }],
          details: { error: "unknown_role" },
        };
      }
      if (resolved.role === "strategos") {
        return {
          content: [{ type: "text", text: "The main session is already the strategos; dispatch a subordinate role instead." }],
          details: { error: "cannot_dispatch_strategos" },
        };
      }

      const agentName = roleName; // canonical agent name (psiloi / lochagos-X / hoplite-X)

      if (!mayDispatch(arch, "strategos", agentName)) {
        return {
          content: [{ type: "text", text: chainOfCommandError(arch, agentName) }],
          details: { error: "chain_of_command" },
        };
      }

      const agent = findAgent(agents, agentName);
      if (!agent) {
        const hint = isLochagosAgent(agentName)
          ? " Specify a domain, e.g. " + (arch.roles.lochagos?.instances ?? []).map((i) => `lochagos-${i}`).join(", ") + "."
          : isHopliteAgent(agentName)
            ? " Known hoplites: " + (arch.roles.hoplites?.direct_reports ?? []).map((d) => `hoplite-${d.id}`).join(", ") + "."
            : "";
        return {
          content: [
            { type: "text", text: `No agent file for role "${agentName}".${hint}\n\nLoaded agents:\n${formatAgentList(agents)}` },
          ],
          details: { error: "no_agent" },
        };
      }

      // tools (exactly one for hoplites)
      let tools = agent.tools ? [...agent.tools] : undefined;
      if (params.tool) tools = [params.tool];
      if (isHopliteAgent(agentName)) {
        if (!tools || tools.length === 0) {
          return {
            content: [{ type: "text", text: `Hoplite "${agentName}" requires exactly one tool. Pass tool: "<name>".` }],
            details: { error: "hoplite_no_tool" },
          };
        }
        if (tools.length > 1) {
          return {
            content: [
              { type: "text", text: `Hoplite "${agentName}" may use exactly one tool (got ${tools.join(", ")}). Split into multiple hoplites.` },
            ],
            details: { error: "hoplite_many_tools" },
          };
        }
      }

      const maxAttempts = Math.max(1, Math.min(Math.round(params.maxAttempts ?? 2), 5));
      const scope = `dispatch:${agentName}`;
      const model = ctx.model ? `${ctx.model.provider}/${ctx.model.id}` : undefined;

      const agoraContext = buildAgoraContext(agora, agentName);
      const task = agoraContext ? `${agoraContext}\n\n---\n\n${params.task}` : params.task;

      await agora.log("dispatch_start", agentName, params.task);

      const runOnce = () =>
        runSubagent({
          cwd: ctx.cwd,
          agentName,
          systemPrompt: agent.systemPrompt,
          task,
          tools,
          model,
          signal,
          onUpdate: (partial) =>
            onUpdate?.({ content: [{ type: "text", text: `[${agentName}] ${partial || "(running...)"}` }] }),
        });

      let lastResult: DispatchResult | undefined;
      for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        await agora.bumpAttempt(scope);
        const r = await runOnce();
        lastResult = r;

        if (!r.isError) {
          await agora.resetAttempt(scope);
          await agora.log("dispatch_ok", agentName, `attempt ${attempt}`);
          return {
            content: [{ type: "text", text: r.output || "(no output)" }],
            details: { agentName, task: params.task, attempts: attempt, exitCode: r.exitCode, model: r.model, turns: r.turns },
          };
        }

        await agora.log(
          "dispatch_fail",
          agentName,
          `attempt ${attempt}: ${r.stopReason ?? r.errorMessage ?? truncate(r.stderr, 200)}`,
        );
        if (attempt < maxAttempts) continue;

        // shield_wall exhausted -> consult the oracle
        const exhaustedMsg =
          `shield_wall exhausted for "${agentName}" after ${maxAttempts} attempt(s). ` +
          `Last error: ${lastResult?.stopReason ?? lastResult?.errorMessage ?? "(none)"}.`;
        if (params.askOracleOnExhaust !== false && ctx.hasUI) {
          const retry = await ctx.ui.confirm(
            "Consult the oracle (user)",
            `${exhaustedMsg}\n\nRetry one more time, or stop and escalate to strategos (you)?`,
          );
          if (retry) {
            const r2 = await runOnce();
            lastResult = r2;
            if (!r2.isError) {
              await agora.resetAttempt(scope);
              await agora.log("dispatch_ok", agentName, "after oracle retry");
              return {
                content: [{ type: "text", text: r2.output || "(no output)" }],
                details: { agentName, task: params.task, attempts: maxAttempts + 1, exitCode: r2.exitCode, model: r2.model },
              };
            }
            await agora.log("dispatch_fail", agentName, "oracle retry failed");
          }
        }

        await agora.log("dispatch_escalate", agentName, exhaustedMsg);
        return {
          content: [
            {
              type: "text",
              text: `${exhaustedMsg}\n\nconsult_the_oracle: stop and ask the user — do not re-dispatch the same scope.`,
            },
          ],
          details: {
            agentName,
            task: params.task,
            attempts: maxAttempts,
            exitCode: lastResult?.exitCode,
            isError: true,
          },
        };
      }

      return { content: [{ type: "text", text: "dispatch failed" }], details: { isError: true } };
    },
  });

  // -------------------------------------------------------------------------
  // phalanx_status tool
  // -------------------------------------------------------------------------
  pi.registerTool({
    name: "phalanx_status",
    label: "Phalanx Status",
    description: "Report the current phalanx state: roles, rules, loaded agents, and agora summary.",
    promptSnippet: "Report phalanx roles, rules, agents, and agora state",
    parameters: Type.Object({}),

    async execute(_toolCallId, _params, _signal, _onUpdate, ctx) {
      const arch = loadArchitecture(ctx.cwd);
      const agents = discoverAgents(ctx.cwd);
      const agora = getAgora(ctx.cwd);
      const snap = agora.snapshot();

      const domains = (arch.roles.lochagos?.instances ?? []).join(", ");
      const hoplites = (arch.roles.hoplites?.direct_reports ?? []).map((d) => d.id).join(", ");
      const rules = arch.rules.map((r) => r.id).join(", ");

      const lines = [
        "# phalanx status",
        "",
        `roles: strategos, psiloi, lochagos (${domains}), hoplites (${hoplites}), agora, oracle`,
        `rules: ${rules}`,
        `agents: ${formatAgentList(agents)}`,
        `agora: ${Object.keys(snap.keys).length} key(s), ${snap.messages.length} message(s), ${snap.log.length} log entry(ies)`,
        `agora path: ${agora.path}`,
      ];
      return text(lines.join("\n"));
    },
  });

  // -------------------------------------------------------------------------
  // commands
  // -------------------------------------------------------------------------
  pi.registerCommand("phalanx", {
    description: "Show phalanx roles, rules, and agora state",
    handler: async (_args, ctx) => {
      const arch = loadArchitecture(ctx.cwd);
      const agora = getAgora(ctx.cwd);
      const snap = agora.snapshot();
      const domains = (arch.roles.lochagos?.instances ?? []).join(", ");
      ctx.ui.notify(
        `phalanx: lochagos (${domains}) | agora: ${Object.keys(snap.keys).length} keys, ${snap.log.length} log entries`,
        "info",
      );
    },
  });

  pi.registerCommand("phalanx add-lochos", {
    description: "Add a lochagos coordinator domain (e.g. /phalanx add-lochos docs)",
    handler: async (args, ctx) => {
      const name = (args ?? "").trim();
      if (!name) {
        ctx.ui.notify("usage: /phalanx add-lochos <domain_name>", "error");
        return;
      }
      const arch = loadArchitecture(ctx.cwd);
      const res = appendLochagosInstance(arch, name.replace(/[^\w-]+/g, "-"));
      if (!res.ok) {
        ctx.ui.notify(res.detail, "error");
        return;
      }
      const dir = ensureAgentsDir(ctx.cwd);
      const file = path.join(dir, `lochagos-${name.replace(/[^\w-]+/g, "-")}.md`);
      fs.writeFileSync(file, lochagosTemplate(name.replace(/[^\w-]+/g, "-")), "utf-8");
      ctx.ui.notify(`${res.detail}\ncreated ${file}`, "info");
    },
  });

  pi.registerCommand("phalanx add-hoplite", {
    description: "Add a hoplite specialist (e.g. /phalanx add-hoplite scribe docs write)",
    handler: async (args, ctx) => {
      const parts = (args ?? "").trim().split(/\s+/).filter(Boolean);
      if (parts.length < 2) {
        ctx.ui.notify("usage: /phalanx add-hoplite <skill_name> <lochagos_id> [tool]", "error");
        return;
      }
      const [id, lochagos, ...rest] = parts;
      const tool = rest.join(" ") || "read";
      const arch = loadArchitecture(ctx.cwd);
      const res = appendHoplite(arch, id, lochagos, tool);
      if (!res.ok) {
        ctx.ui.notify(res.detail, "error");
        return;
      }
      const dir = ensureAgentsDir(ctx.cwd);
      const file = path.join(dir, `hoplite-${id.replace(/[^\w-]+/g, "-")}.md`);
      fs.writeFileSync(file, hopliteTemplate(id.replace(/[^\w-]+/g, "-"), lochagos, tool), "utf-8");
      ctx.ui.notify(`${res.detail}\ncreated ${file}`, "info");
    },
  });

  pi.registerCommand("phalanx reset", {
    description: "Clear the agora shared memory (keys, messages, log, attempts)",
    handler: async (_args, ctx) => {
      const agora = getAgora(ctx.cwd);
      await agora.clear();
      ctx.ui.notify("agora cleared", "info");
    },
  });

  // -------------------------------------------------------------------------
  // rules injection (roles -> rules, per the architecture read order)
  // -------------------------------------------------------------------------
  pi.on("before_agent_start", async (event, ctx: ExtensionContext) => {
    const arch = loadArchitecture(ctx.cwd);
    const agents = discoverAgents(ctx.cwd);
    const fragment = buildRulesFragment(arch) + "\n\n" + buildRosterFragment(arch, agents);
    return { systemPrompt: event.systemPrompt + "\n\n" + fragment };
  });
}
