# PERF-06 browser probe — tasks

- [x] Re-review current probe and locate the missing real-browser CI coverage.
- [x] Add an isolated HTTP fixture with decodable image and conditional ETag/304.
- [x] Add CLI subprocess tests for initial/repeat capture, lack of URL leakage and missing-photo failure.
- [x] Expose a bounded image-load timeout and cover parser validation.
- [x] Run targeted browser probe in CI after Chromium is installed.
- [x] Update performance usage documentation and OpenSpec traceability.
- [ ] Review CI failures and adjust until the final PR head is green.
- [ ] Merge only after successful CI; synchronize audit row with actual evidence.
