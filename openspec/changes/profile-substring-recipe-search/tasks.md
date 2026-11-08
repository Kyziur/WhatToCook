## Verification

- [x] Build PostgreSQL synthetic 1k/10k seed and EXPLAIN benchmark with controlled non-production GIN index.
- [x] Compare query counts and page identities at equal data sizes for selective, common and short phrases.
- [x] Cover the public HTTP search contract without introducing a production index.
- [x] Add a reproducibility guide and CI job output for the performance diagnostic.
- [ ] Review real CI plans/results, record evidence and decide whether further production work is warranted.
