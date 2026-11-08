# AGENT-03 CI and review evidence — 2026-11-08

- [PR #36](https://github.com/Kyziur/WhatToCook/pull/36), verified implementation SHA `5c37eaa0ce48deceabeade18d72ef8a5a71f05ed`.
- [PR CI run 42507263204](https://github.com/Kyziur/WhatToCook/actions/runs/42507263204): **success** for `verify` and ARM64 upload-smoke jobs.
- `node --test tests/ci/repowise-config.test.mjs tests/ci/agent-skill-references.test.mjs`: **3/3 passed**, including named-skill navigation contract.
- API integration tests: **118 passed**; Blazor component tests: **53 passed**; Playwright smoke: **98 passed**, remaining opt-in/design-only tests skipped.
- Change constrained to the `run-tests` skill, its targeted CI check, and OpenSpec/audit documentation. No production API, service, database or deployment contract changed.

## Review conclusion

Five previously unresolved named skill links were replaced with existing in-file guidance, actual CI instructions, or installed `systematic-debugging`. The test intentionally validates actionable backtick-quoted references in `run-tests` only; this does **not** assert that every skill in the entire repository is free of indirect references or that AGENT-03 broad deduplication is needed.

Independent review and explicit merge remain open. Final-head documentation-only CI must be observed before merge; updating this file does not replace that check.

## Subsequent command-mode review finding

The review identified a second concrete conflict: the skill prohibited positional paths and `--` for *all* .NET SDK 10 calls, despite WhatToCook using VSTest command mode on SDK 10. Corrected the distinction between VSTest, VSTest-to-MTP bridge, and native MTP (including the optional native `--` separator) against [Microsoft's testing guide](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test). Added a focused source-contract regression check; **final-head CI for this subsequent correction is pending**.
