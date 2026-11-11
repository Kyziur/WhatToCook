# Self-hosted deployment — operator handoff

**Status:** `deploy/komodo.toml` is a **candidate** independent Resource Sync owner. Committing it does **not** activate the Sync or prove a successful Pi migration. Follow the operator-approved procedure in [Kyziur/homelab](https://github.com/Kyziur/homelab); this repository contains the [application Compose contract](../deploy/docker-compose.yml), not the live deployment controller.

## Who owns what

- **WhatToCook:** API/Web images, `deploy/docker-compose.yml`, `deploy/komodo.toml`, schema migrations and application health behavior.
- **Homelab:** Komodo/Periphery, Bitwarden Secrets Manager (BWS), host preparation, shared networks, ingress, locks, backups, restore and runtime verification.

The Compose project is configured as `whattocook` in the candidate manifest. The logical database volume is `postgres-data` (expected Docker name `whattocook_postgres-data`); **verify the actual existing live identity** before applying anything. The default host `DATA_ROOT` candidate is `/srv/whattocook`, not evidence of the current host state.

## Preflight — stop on any mismatch

Before transferring ownership or deploying, the authorized homelab operator must:

1. Compare **legacy and new Resource Sync previews**; confirm one owner of the `what-to-cook` Stack and no unexpected deletes or duplicate Compose projects.
2. Record the existing project name, PostgreSQL volume and data root. Confirm a **tested, isolated restore** of the database **and** recipe images. A backup file alone is not enough.
3. Check actual shared networks (`edge`, `observability`, `egress`), Web-only edge reachability and proxy WebSocket/forwarded-header support. API and PostgreSQL must not be externally exposed.
4. Verify `${DATA_ROOT}/recipe-images` is a real, expected directory, writable by API UID/GID **1654:1654** with mode **0750**. Check the homelab-owned `prepare-what-to-cook` hook for unsafe symlink/path traversal: it has host-root capabilities.
5. Confirm maintenance locking and a documented, verified recovery path. The candidate wraps pre-deploy and `config/build/pull/up/run` with the homelab lock; **manual `down` is not covered**.

**STOP:** missing restore evidence, unexpected volume/project changes, an unsafe host-preparation hook, duplicate Sync ownership, missing networks or surprise exposure. Do not use a fresh empty volume as a fallback.

## Configuration and application constraints

[The example](../deploy/.env.example) lists non-secret Stack variables; [Compose](../deploy/docker-compose.yml) is authoritative for required/optional values. `DATA_ROOT` is mandatory. `POSTGRES_PASSWORD` must come only from the existing homelab BWS workload wrapper, **not** Git, a committed `.env` or a Komodo Stack environment field. `BWS_ACCESS_TOKEN` belongs to the host, never the application Stack.

Compose builds Web/API from the checkout; it does **not** pin immutable published images. The API applies EF Core migrations at startup: deploy **one controlled API replica**, with a reviewed schema-recovery plan. Rotating a BWS secret alone does not update the existing PostgreSQL role password.

## After an approved deployment

Verify real healthchecks, a **database-backed** recipe read/write, recipe-photo upload/read, Web SignalR connection, network isolation, expected existing database contents and a fresh backup/restore check. Record the actual outcome in the homelab rollout evidence.

If verification fails, stop further changes and follow the **previously rehearsed homelab recovery procedure**. This page does not claim that reverting a Git commit automatically reverses migrations or restores data. Local development uses [Aspire](../README.md#run-locally), not this production procedure.
