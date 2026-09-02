---
name: phalanx-psiloi
description: Dispatch a psiloi scout for fast, cheap codebase reconnaissance before committing any specialist. Use when you need to locate files, symbols, or understand structure quickly.
---

# Psiloi (scout)

A **psiloi** is a read-only scout: `read, grep, find, ls`. It is the cheap probe
you run *before* committing a lochagos or hoplite.

## When to dispatch

- You don't know where something lives in the codebase.
- You need a map of files/symbols before a build task.
- You want to avoid burning a specialist on pure discovery.

## How

```
phalanx_dispatch { role: "psiloi", task: "locate all authentication code and return file paths + line numbers" }
```

## Rules

- `scout_first` — this is the first dispatch for unknown territory.
- Read-only; the scout never modifies files.
- Compressed, precise output: paths, line refs, minimum context.
