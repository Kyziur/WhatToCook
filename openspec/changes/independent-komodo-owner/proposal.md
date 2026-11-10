# Independent WhatToCook Resource Sync ownership

## Problem
The legacy homelab Resource Sync manages the WhatToCook Stack, coupling an application release to platform resources. The clean rebuild needs six independently reviewable Sync owners.

## Scope
- Stage one `deploy/komodo.toml` entry for the existing Stack and Compose project identity.
- Preserve host-owned BWS/Periphery wrapper, predeploy contract and existing `postgres-data` volume.
- Add fast source validation and safe handoff notes; no actual Core or Pi operations.

## Non-goals
No database migration, no runtime Sync import or deletion, no automatic deploy, no credential migration.
