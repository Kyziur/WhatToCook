# OBS-02 — Verify recipe image storage readiness

## Why
The API readiness endpoint checks PostgreSQL connectivity but not its configured bind-mounted recipe image directory. A deployment can report ready even when uploads cannot write images.

## What changes
Register a small image-storage health check tagged `ready`. Check the configured directory by creating a unique temporary file, writing and reading one byte, and deleting the file on close. Keep `/alive` process-only.

## Scope
No new host hooks, migrations, network exposure, backup, BWS secrets, image processing, or persistent test data. The homelab still owns mount creation and UID/GID 1654:1654. A passing automated check does not replace a real host upload/read drill.
