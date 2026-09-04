---
name: phalanx-psiloi
description: Dispatch a psiloi scout for fast, cheap codebase reconnaissance when you don't know where to act. Use when you need to locate files, symbols, or understand structure before dispatching a lochagos — skip it when you already know the target.
---

# Psiloi (scout)

A **psiloi** is a read-only scout: `read, grep, find, ls`. It is the cheap probe
you run when you don't yet know where to act — not a mandatory first step.

## When to dispatch

- You don't know where something lives in the codebase.
- You need a map of files/symbols before a build task.
- You want to avoid burning a lochagos dispatch on pure discovery.

Skip it entirely when you already know the target — go straight to acting
directly or dispatching a lochagos.

## How

```
phalanx_dispatch { role: "psiloi", task: "locate all authentication code and return file paths + line numbers" }
```

## Rules

- `scout_first` — opt-in: dispatch when the target is unknown, skip when it's already known.
- Read-only; the scout never modifies files.
- Compressed, precise output: paths, line refs, minimum context.
