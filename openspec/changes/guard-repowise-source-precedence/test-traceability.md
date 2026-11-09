# AGENT-01 partial — traceability

| Requirement | Verification | Status |
| --- | --- | --- |
| Current source precedence is maintained above the generated marker | `tests/ci/repowise-config.test.mjs` | Pending CI |
| Existing generated section remains untouched by this change | Scoped diff of `AGENTS.md` | Pending review |
| Agent preflight can distinguish local checkout from live indexed scope | `git rev-parse HEAD`, `repowise status --format json`, MCP response metadata | Manual host/session verification pending |
