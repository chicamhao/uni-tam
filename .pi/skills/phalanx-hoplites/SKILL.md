---
name: phalanx-hoplites
description: Dispatch a hoplite specialist for exactly one task with exactly one tool. Use for a small, well-scoped job that a full coordinator would be overkill for.
---

# Hoplites (specialist)

A **hoplite** executes exactly one task with exactly one tool. A task needing two
tools becomes two hoplites.

Two hoplites are direct reports that bypass the lochagoi (they are cross-cutting):

| Hoplite | Job |
|---------|-----|
| `hoplite-kerux` | keeps the strategos note and `Scripts/` in sync, either direction, only when told |
| `hoplite-nomophylax` | applies changes to `phalanx-architecture.yaml` via the extend templates, then logs |

## When to dispatch

- A single, well-scoped task (one tool).
- You want isolation for a risky edit.

## How

```
phalanx_dispatch { role: "hoplite-nomophylax", task: "add a verify lochagos to the architecture", tool: "edit" }
```

## Rules

- Exactly one tool per hoplite — enforce it with the `tool` parameter.
- `chain_of_command` — hoplites report to their lochagos (or, for direct reports, to you).
- `shield_wall` — retry once at the narrowest scope, then escalate.
