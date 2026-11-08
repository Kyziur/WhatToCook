# ARC-03 partial — tasks

- [x] Find the archive-preservation flaw in the existing plan-update HTTP handler.
- [x] Move the retained-assignment identity rule into an internal pure policy, retaining the existing transaction and lock scope.
- [x] Add DB-free rule test plus PostgreSQL integration tests for retained, moved and swapped inactive recipe assignments.
- [x] Update OpenSpec and audit status as partial/under review.
- [ ] Validate final-head CI and iterate on any issues, then record evidence.
- [ ] Assess a full use-case extraction only if endpoint changes require it; do not refactor speculatively.
