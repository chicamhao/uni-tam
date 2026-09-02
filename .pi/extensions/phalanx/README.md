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
- **Commands** — `/phalanx`, `/phalanx add-lochos <domain>`,
  `/phalanx add-hoplite <skill> <lochagos> [tool]`, `/phalanx reset`.

## Role agents (`.pi/agents/`)

| Agent | Tier | Tools | Purpose |
|-------|------|-------|---------|
| `psiloi` | scout | read, grep, find, ls | cheap recon, run first |
| `lochagos-research` | coordinator | read, grep, find, ls, bash | investigate a domain |
| `lochagos-build` | coordinator | read, edit, write, bash | implement a domain |
| `lochagos-verify` | coordinator | read, grep, bash | verify a domain |
| `hoplite-kerux` | specialist (direct) | read, edit, write | keep note ↔ `Scripts/` in sync |
| `hoplite-nomophylax` | specialist (direct) | read, edit | edit `phalanx-architecture.yaml` via extend templates |

The main session **is** the strategos. It dispatches down and owns final decisions.

## Chain of command

- strategos → `psiloi`, `lochagos-*`, and the two direct-report hoplites.
- lochagos → hoplites (one task, one tool each).
- psiloi / hoplites dispatch nothing; they escalate up.

## Extending the phalanx

```
/phalanx add-lochos docs                        # new coordinator domain
/phalanx add-hoplite scribe docs write          # new specialist under docs
```

Both commands append to `phalanx-architecture.yaml` (using the `extend`
templates) and create the matching `.pi/agents/*.md` file.

## Layout

```
.pi/
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
