## Verification

- [x] Add real-browser reconnect/unsaved recipe editor regression.
- [x] Add real-browser reconnect/unsaved plan form regression.
- [x] Assert a single persisted recipe/plan after restoring connectivity.
- [x] Fix the reproduced focused-step divergence by updating the Blazor editor step model on input (not only on blur).
- [ ] Verify desktop and mobile Playwright via PR CI, review failures, and document evidence.

No application code behavior change is planned unless the tests uncover lost drafts.
