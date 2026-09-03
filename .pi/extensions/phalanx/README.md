# Phalanx extension

Implements `phalanx-architecture.yaml` as a native pi extension plus role
skills. Read order mirrors the file: **roles → rules → extend**.

## What it gives you

- **`agora`** — shared memory + message bus (the *single_state* rule). File-backed
  at `.pi/phalanx/agora.json` so dispatched subagents can share state.
- **`phalanx_dispatch`** — dispatches a role as an isolated `pi` subprocess with
  restricted tools. Enforces *chain_of_command*, *shield_wall* (retry once, then
  escalate), and *consult_the_oracle* (asks you when retries are exhausted).
- **`phalanx_status`** — reports roles, rules, loaded agents, and agora state.
- **Commands** — `/phalanx`, `/phalanx-new`.

## Strategos prompt (`.pi/agent/AGENTS.md`)

The main session (strategos) gets its role declaration from `.pi/agent/AGENTS.md` — a
per-project override of the global `~/.pi/agent/AGENTS.md`. This file tells Pi
it is the **strategos**, lists the six phalanx rules, and names every dispatchable
agent in the project.

Without this file, Pi has no built-in knowledge of the phalanx role — it defaults
back to "expert coding assistant" and won't consistently dispatch.

**Both** the global AND project-level file should be kept in sync. The global one
covers every Pi session; the project one adds the specific roster for `ares`.

## Role agents (`.pi/agents/`)

| Agent | Tier | Tools | Purpose |
|-------|------|-------|---------|
| `psiloi` | scout | read, grep, find, ls | cheap recon, run first |
| `lochagos-research` | coordinator | read, grep, find, ls, bash | investigate a domain |
| `lochagos-build` | coordinator | read, edit, write, bash | implement a domain |
| `lochagos-verify` | coordinator | read, grep, bash | verify a domain |


The main session **is** the strategos. It dispatches down and owns final decisions.

## Chain of command

- strategos → `psiloi`, `lochagos-*`, and direct-report hoplites.
- lochagos → hoplites (one task, one tool each).
- psiloi / hoplites dispatch nothing; they escalate up.

## Extending the phalanx

Edit `phalanx-architecture.yaml` and update the strategos prompt and
README to reflect any added roles.

## Layout

```
.pi/
├── agent/AGENTS.md                 # 🆕 Strategos system prompt (overrides global)
├── extensions/phalanx/
│   ├── index.ts        entry — tools, commands, rules injection
│   ├── architecture.ts YAML parser + model + extend helpers
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
