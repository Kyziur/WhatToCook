# ARC-01 targeted — evidence

## Confirmed failure and scoped correction
Before #44, the editor persisted a new recipe and then attempted the optional photo upload. If upload failed, the form stayed on the create route and a subsequent save called `SaveAsync(null, ...)` again. The editor now stores the successful create response ID before uploading and reuses that ID for retries. Existing editing and no-photo navigation remain unchanged.

## Final verification
- [PR #44](https://github.com/Kyziur/WhatToCook/pull/44), squash merge `c0af5e08c1ca971ec2da4d9f27517020bdea5398`.
- **Final-head [CI run 42599607557](https://github.com/Kyziur/WhatToCook/actions/runs/42599607557) passed**: 126 API integration, 65 component, 98 Playwright browser tests, plus ARM64 upload smoke.
- New bUnit theory exercised both a returned upload failure and a thrown exception followed by a retry; both verify the second save uses the original ID. Existing no-photo create remains the control.
- Earlier CI attempts failed CSharpier formatting; formatting was corrected in separate commits before final green CI. This does not claim a production smoke or an independent human review.

## Limits
Only the current Blazor editor session retains the ID. Lost initial create responses, circuit loss, reload recovery, exactly-once server-side idempotency and page-wide extraction are not addressed. ARC-01 remains **PARTIAL · CI**.
