---
name: phalanx-oracle
description: Escalate to the oracle (the user) when the objective is ambiguous or retries are exhausted. Use when you must not guess and must not re-dispatch the same failing scope.
---

# Oracle (escalation)

The **oracle** is the user. It sits outside the chain of command. Only the strategos
(you) may consult it — and only when the system cannot resolve the situation.

## When to consult

- The objective is ambiguous and guessing would be costly.
- A `shield_wall` escalation has reached you and there is nothing new to try.
- A subagent has exhausted its retries and you have no better task to give it.

## How

`phalanx_dispatch` consults the oracle automatically when a role's retries are
exhausted (you can disable this with `askOracleOnExhaust: false`). Otherwise, stop
and ask the user directly — present the ambiguity or the failed attempts and ask for
a decision.

## Rules

- `consult_the_oracle` — ask; do not re-dispatch the same scope.
- Keep interruptions rare: resolve everything resolvable first, then ask once with
  a clear, decision-ready question.
