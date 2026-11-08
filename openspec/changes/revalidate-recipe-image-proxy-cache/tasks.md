# PERF-06 partial — HTTP image revalidation

- [x] Audit API/Web image serving; confirm header propagation and 304 gap.
- [x] Implement Web validator forwarding, private revalidation headers and upstream 304 handling.
- [x] Extend Playwright recipe-photo test to check 200/ETag/304.
- [x] Update OpenSpec, performance documentation and audit as partial/under review.
- [ ] Run and review final-head PR CI, capture evidence and fix any discovered failures.
- [ ] Obtain representative image/network transfer samples before considering resized variants.
