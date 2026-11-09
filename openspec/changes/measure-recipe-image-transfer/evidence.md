# PERF-06 partial — probe evidence

- Existing #41 verified conditional requests and no-body 304 responses. That transport check is not a performance baseline.
- New Node tests verify CLI argument bounds and pure browser ResourceTiming aggregation; a browser probe can be run manually on a **real-image** library using the documented command.
- **No real-photo, mobile network, household host, or LCP comparative measurement has been performed here.** CI validation is pending.
- No automatic thumbnail generation or image API/storage behavior was changed.
