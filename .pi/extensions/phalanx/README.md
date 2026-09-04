# Phalanx extension

Implements `phalanx-architecture.yaml` as a native pi extension plus role
skills. Read order mirrors the file: **roles → rules → extend**.

## What it gives you

- **`agora`** — shared memory + message bus (the *single_state* rule). File-backed
  at `.pi/phalanx/agora.json` so dispatched subagents can share state.
- **`phalanx_dispatch`** — dispatches a role as an isolated `pi` subprocess with
  restricted tools. Enforces *chain_of_command*, *shield_wall* (retry once, then
  once more on an escalation model if configured, then escalate), and
  *consult_the_oracle* (asks you when retries are exhausted). Scopes agora
  context per dispatch via `contextKeys` instead of broadcasting the whole store.
- **`phalanx_status`** — reports roles, rules, loaded agents, and agora state.
- **Commands** — `/phalanx`, `/phalanx-new`.

## Strategos prompt (`.pi/agent/AGENTS.md`)

The main session (strategos) gets its role declaration from `.pi/agent/AGENTS.md` — a
per-project override of the global `~/.pi/agent/AGENTS.md`. This file tells Pi
it is the **strategos**, lists the six phalanx rules, and names every dispatchable
agent in the project.

Without this file, Pi has no built-in knowledge of the phalanx role — it defaults
back to "expert coding assistant" and won't consistently act as strategos.

**Both** the global AND project-level file should be kept in sync. The global one
covers every Pi session; the project one adds the specific roster for `ares`.

## Role agents (`.pi/agents/`)

| Agent | Tier | Tools | Purpose |
|-------|------|-------|---------|
| `psiloi` | scout | read, grep, find, ls | cheap recon, when target is unknown |
| `lochagos-work` | coordinator | read, edit, write, grep, find, ls, bash | generalist — default for small/medium tasks |
| `lochagos-research` | coordinator | read, grep, find, ls, bash | investigate a domain (large-effort split) |
| `lochagos-build` | coordinator | read, edit, write, bash | implement a domain (large-effort split) |
| `lochagos-verify` | coordinator | read, grep, bash | verify a domain (large-effort split) |


The main session **is** the strategos. It is a planner and reporter: it
dispatches down for the work and owns final decisions, acting directly only
for trivial one-step lookups.

## Chain of command

- strategos → `psiloi`, `lochagos-*`.
- lochagos / psiloi dispatch nothing further; they work directly and escalate up.

## Extending the phalanx

Edit `phalanx-architecture.yaml` and update the strategos prompt and
README to reflect any added roles.

## Layout

```
.pi/
├── agent/AGENTS.md                 # 🆕 Strategos system prompt (overrides global)
├── extensions/phalanx/
│   ├── index.ts        entry — tools, commands, rules injection
│   ├── architecture.ts YAML parser + typed model
│   ├── agora.ts        shared memory + message bus
│   ├── agents.ts       role agent discovery
│   └── dispatch.ts     isolated subagent runner
├── agents/             role system prompts (subagents)
├── skills/phalanx-*/   role skills (loaded on demand by the strategos)
└── phalanx/agora.json  runtime state (gitignored)
```

## Notes

- Subagents run with `--no-extensions`, so a scout only gets its own prompt and
  tools — not the strategos roster.
- `agora` state is a shared bus, not branch-local: it is the single source of
  truth across the main session and every subagent process.
