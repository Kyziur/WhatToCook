# CI-04 / OPS-04 — Deployment-backed database and photo smoke

## Problem
The existing Compose integration test demonstrates that Web starts, but does not prove recipe persistence or photo writes through the deployed non-root API's host bind mount.

## Scope
Extend the existing isolated Compose integration test: expose API **only** on host loopback in the disposable test override, create/read a throwaway recipe via HTTP and upload/read a test GIF. The production Compose file, networking, credentials, volume identity and host preparation remain unchanged.

## Limits
This verifies an ephemeral CI test mount. It does **not** prove the actual Raspberry Pi mount is writable by UID/GID 1654:1654; an operator must perform the equivalent deployed host smoke separately. The test destroys only its own randomly named Compose project and temporary data directory.
