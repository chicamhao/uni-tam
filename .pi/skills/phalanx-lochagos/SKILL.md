---
name: phalanx-lochagos
description: Dispatch a lochagos coordinator to break an objective into tasks for one domain (research, build, or verify). Use for substantial, multi-step work in a single domain.
---

# Lochagos (coordinator)

A **lochagos** coordinates one domain. There is one per domain:

| Domain | Agent | Role |
|--------|-------|------|
| research | `lochagos-research` | investigate and locate |
| build | `lochagos-build` | implement changes |
| verify | `lochagos-verify` | test and confirm correctness |

## When to dispatch

- The objective is substantial and belongs to one domain.
- You want one coordinator to break it into tasks and carry them out in an isolated
  context.

## How

```
phalanx_dispatch { role: "lochagos-build", task: "implement the X feature in Scripts/" }
```

## Rules

- `chain_of_command` — the lochagos reports to you and dispatches hoplites in its
  domain. It never works sideways into another domain.
- `shield_wall` — on failure, it retries once at the narrowest scope, then escalates to you.
