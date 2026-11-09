# AGENT-01 partial — current-source precedence evidence

- [PR #46](https://github.com/Kyziur/WhatToCook/pull/46), squash merge `f6ed469a3997471d0475ccab83fbffc223b70674`.
- **Final-head [CI run 42599543617](https://github.com/Kyziur/WhatToCook/actions/runs/42599543617) passed**: Node Repowise marker/precedence checks, 126 API, 63 component, 98 Playwright tests and ARM64 image smoke.
- The checked-in generated Repowise region contains an unconditional trust-index claim, which conflicts with maintained source-first guidance when index freshness is uncertain. The maintained instructions now explicitly take precedence, and a Node test guards the marker and core freshness rule.
- Source diff review confirmed no change within generator-owned Repowise region.

## Limits
The generated footer's old revision is **not evidence** that the live Repowise MCP index is stale. Per-task live index status/scope checks remain necessary. No actual second-checkout MCP-client smoke or independent human review was performed. AGENT-01 remains **PARTIAL · CI**.
