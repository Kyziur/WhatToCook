# AGENT-01 (partial) — Current source wins over generated Repowise instructions

## Why
The checked-in generated Repowise region currently says to trust verified indexed bytes and never re-read them, while the human-maintained instructions require checking live freshness and source. Such conflicting guidance can cause agents to reason from a stale static index. The generated footer has an older snapshot revision; this is not proof about the live MCP index.

## Changes
- Make precedence explicit **above** the generated region: maintained current-source/freshness policy overrides any conflicting generated trust instruction.
- Add a Node CI regression for marker containment and required maintained freshness guidance, without pinning a generated index revision/date.
- Document a short task-time index preflight via `git rev-parse HEAD`, `repowise status --format json`, and live tool metadata.

## Non-goals
No hand-editing generated text, no forced automatic index regeneration, MCP-host cwd or second checkout test, new agent orchestration, or assumption that the live Repowise index is currently stale. AGENT-01 remains partial until agent sessions consistently verify live metadata.
