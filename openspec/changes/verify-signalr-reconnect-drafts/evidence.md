# UX-03 reconnect verification

**Code head:** `5a1c4d1f899233d91e7ea13db79f742beb7ceb04` (2026-11-08).

- [GitHub Actions run 42477078184](https://github.com/Kyziur/WhatToCook/actions/runs/42477078184): both `verify` and ARM64 image smoke jobs succeeded. API integration tests: 114 pass; component tests: 53 pass; desktop/mobile Playwright: 98 pass; NuGet/npm audit and CSharpier pass.
- Both recipe editor and new-plan scenarios were verified in Chromium desktop and mobile. Network interruption is simulated by routing Blazor WebSockets before page navigation, explicitly closing the active socket, rejecting reconnection for ten seconds and then restoring the connection.
- The recipe test verifies visible title, servings, ingredient and focused step after reconnection, then exactly one persisted recipe; the planner test verifies plan name/date range and exactly one created plan.
- **Review iteration 1:** the first offline-only simulation did not close the existing WebSocket, so reconnect was never triggered. Switched to deterministic WebSocket route closure (commit `a8c7f6ce877c63548f961bc3d0d5524615949ced`).
- **Review iteration 2:** the real interruption exposed that a focused `InputTextArea` can retain visible text while the server-side step model remains empty until `change`/blur. The save then rejected the recipe with `Dodaj co najmniej jeden krok`. Added a small `@oninput` handler that updates the existing step model and dirty state while the user types (commit `5a1c4d1f899233d91e7ea13db79f742beb7ceb04`). The subsequent full CI was green.

**Scope limitations:** this proves a brief (ten-second) browser-side network interruption with a retained Blazor circuit, not server crash/eviction, hours-long offline work, or the reverse-proxy configuration in homelab. No general offline draft persistence was introduced.
