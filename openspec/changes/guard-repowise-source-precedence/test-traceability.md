# AGENT-01 partial — traceability

| Requirement | Verification | Status |
| --- | --- | --- |
| Current source precedence is maintained above the generated marker | `tests/ci/repowise-config.test.mjs` | [CI 42599543617](https://github.com/Kyziur/WhatToCook/actions/runs/42599543617) passed |
| Existing generated section remains untouched by this change | Scoped diff of `AGENTS.md` | Author-side diff review confirmed; generated region unchanged |
| Agent preflight can distinguish local checkout from live indexed scope | `git rev-parse HEAD`, `repowise status --format json`, MCP response metadata | Manual host/session verification pending |
