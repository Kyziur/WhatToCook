# Guard repository audit tracker status totals

## Why
Several small PRs edit the same 69-finding live audit tracker, including status rows and a manually maintained summary count. During parallel PR reconciliation, the status counters can become inaccurate even when every individual PR passes application tests.

## Scope
- Add a dependency-free Node test that counts the original 69 finding rows, checks unique identifiers, parses allowed status values, and compares totals to the tracker heading.
- Run this focused check **before** the slower .NET and browser CI stages.
- Preserve existing audit statuses, original findings and review/merge workflow. Trackers must be updated together with any status change.

## Out of scope
No code generator, no auto-rewriting audit findings, no new original audit entries or changes to status semantics; no proof that evidence linked from a status row is correct.

## Failure modes
A PR changing a finding's status without updating counts, duplicating an identifier or accidentally deleting a row must fail fast with an actionable Node assertion.
