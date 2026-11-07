## Why

CI-07 found assertions of deployment documentation wording that reject harmless prose changes but do not verify the actual Compose deployment contract.

## What Changes

- Replace brittle documentation-phrase assertions with `docker compose config` validation of effective service network boundaries, API base URL, image bind mount, database volume and required environment.
- Keep the existing container startup integration test and its teardown.
- Verify that a missing required mount variable causes Compose rendering to fail.

## Non-Goals

No change to production Compose, host mounts, Komodo, BWS, credentials, or real backup/restore testing. Those are homelab-owned.
