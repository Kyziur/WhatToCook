# Audit tracker integrity

- [x] Identify repeated tracker reconciliation risk in parallel PRs.
- [x] Add focused Node tests for unique IDs, exact row count, status counts and partial-in-review classification.
- [x] Register the check before build and other expensive PR CI stages.
- [ ] Confirm final-head CI, record evidence and review diff.
- [ ] Merge after successful checks; do not alter original finding statuses.
