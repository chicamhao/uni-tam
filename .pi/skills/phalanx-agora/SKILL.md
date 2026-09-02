---
name: phalanx-agora
description: Use the phalanx agora for shared memory and messaging — the single source of state. Use whenever coordinating work across multiple dispatches so nothing is lost between them.
---

# Agora (shared memory + message bus)

The **agora** is the only place state lives (`single_state`). No agent keeps private
state; every read and write goes through the `agora` tool.

## Key/value store

- `agora { action: "put", key: "objective", value: "{\"text\": \"...\"}" }`
- `agora { action: "get", key: "objective" }`
- `agora { action: "list" }`
- `agora { action: "del", key: "..." }`

`value` is a JSON string: `"hello"` stores a string, `{"a":1}` stores an object.

## Message bus

- `agora { action: "post", from: "strategos", to: "lochagos-build", content: "..." }`
- `agora { action: "inbox", to: "strategos" }`
- `agora { action: "mark_read", id: "msg-..." }`

## Retry tracking (shield_wall)

- `agora { action: "attempts", scope: "dispatch:lochagos-build" }`

## Rules

- Write the objective and any decision to agora so later dispatches see it.
- Read agora before dispatching so a subagent inherits shared context.
- Never keep the same fact only in the conversation — record it in agora.
