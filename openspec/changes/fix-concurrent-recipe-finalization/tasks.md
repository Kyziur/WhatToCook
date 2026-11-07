## Implementation
- [x] Acquire draft row lock before finalization reads; make the operation transactional.
- [x] Test ten concurrent HTTP finalizations against PostgreSQL, including same recipe ID and exactly one created response.
- [ ] Validate release CI and review, then leave PR open for approval.
