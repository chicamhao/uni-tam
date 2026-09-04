---
name: phalanx-lochagos
description: Dispatch a lochagos coordinator — lochagos-work for a self-contained small/medium objective, or lochagos-research/build/verify to split a large multi-file effort across dedicated passes. The strategos plans and reports; this is how it gets work done.
---

# Lochagos (coordinator)

A **lochagos** coordinates one domain in an isolated context, working directly
(it dispatches nothing further):

| Domain | Agent | Role |
|--------|-------|------|
| work | `lochagos-work` | generalist — investigate, implement, and verify in one pass; default for small/medium tasks |
| research | `lochagos-research` | investigate and locate |
| build | `lochagos-build` | implement changes |
| verify | `lochagos-verify` | test and confirm correctness |

## When to dispatch

- The strategos is a planner and reporter — it dispatches a lochagos for
  virtually all real work, and only acts directly for trivial one-step lookups.
- **Small/medium, self-contained** — dispatch `lochagos-work` alone.
- **Large, multi-file effort** — split across `lochagos-research` →
  `lochagos-build` → `lochagos-verify` as separate passes.

## How

```
phalanx_dispatch { role: "lochagos-work", task: "add the X feature to Scripts/" }
```

## Rules

- `chain_of_command` — the lochagos reports to you and works its domain directly;
  it dispatches nothing further. It never works sideways into another domain.
- `shield_wall` — on failure, it retries once at the narrowest scope, then
  escalates to you.
