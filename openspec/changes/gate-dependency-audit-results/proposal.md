## Why

SEC-08 found that the existing dependency scan commands did not establish a fail-closed, retained vulnerability policy. NuGet's vulnerability listing is primarily a report; seeing a successful workflow step does not prove a High/Critical advisory would fail CI.

## What Changes

- Make High/Critical NuGet advisories in direct and transitive packages fail the PR workflow; preserve Low/Moderate visibility.
- Preserve existing High/Critical npm audit policy and retain both machine-readable reports, even on failure.
- Add a small dependency-free Node gate with positive, negative and malformed-report tests; document the policy and limitations.

## Non-Goals

No new scanner, secret-scanning/SAST/SBOM claim, dependency upgrade, production host or homelab configuration change.
