# Self-hosted deployment — operator handoff

**Status:** `deploy/komodo.toml` is a **candidate** independent Resource Sync owner. Committing it does **not** activate the Sync or prove a successful Pi migration. Follow the operator-approved procedure in [Kyziur/homelab](https://github.com/Kyziur/homelab), starting with the [migration runbook](https://github.com/Kyziur/homelab/blob/main/docs/migration-runbook.md); this repository contains the [application Compose contract](../deploy/docker-compose.yml), not the live deployment controller.

## Who owns what

- **WhatToCook:** API/Web images, `deploy/docker-compose.yml`, `deploy/komodo.toml`, the Stack pre-deploy hook in `scripts/prepare-what-to-cook/`, schema migrations and application health behavior.
- **Homelab:** Komodo/Periphery, Bitwarden Secrets Manager (BWS), the maintenance lock, shared networks, ingress, backups, restore and runtime verification.

The Compose project is configured as `whattocook` in the candidate manifest. The logical database volume is `postgres-data` (expected Docker name `whattocook_postgres-data`); **verify the actual existing live identity** before applying anything. The default host `DATA_ROOT` candidate is `/srv/whattocook`, not evidence of the current host state.

## Preflight — stop on any mismatch

Before transferring ownership or deploying, the authorized homelab operator must:

1. Compare **legacy and new Resource Sync previews**; confirm one owner of the `what-to-cook` Stack and no unexpected deletes or duplicate Compose projects.
2. Record the existing project name, PostgreSQL volume and data root. Confirm a **tested, isolated restore** of the database **and** recipe images. A backup file alone is not enough.
3. Check actual shared networks (`edge`, `observability`, `egress`), Web-only edge reachability and proxy WebSocket/forwarded-header support. API and PostgreSQL must not be externally exposed.
4. Verify `${DATA_ROOT}/recipe-images` is a real, expected directory, writable by API UID/GID **1654:1654** with mode **0750**. Compose's `create_host_path: false` only stops Compose from creating the bind source, so a missing directory fails the Compose deployment. It does not control the `pre_deploy` step: `flock -x -w 900 /etc/komodo/.homelab-maintenance.lock ../scripts/prepare-what-to-cook/prepare-what-to-cook .env` (WhatToCook-owned, runs with host-root capabilities) is what may create or change ownership of that directory. Verify `/srv/whattocook` is root-owned and not group- or other-writable on the host, and that the Stack revision (branch `main`) contains the reviewed hook. Docker Desktop has also created a missing shared path even for `docker run --mount`, so do not rely on `create_host_path` alone.
5. Confirm maintenance locking and a documented, verified recovery path. The candidate wraps `pre_deploy` (explicit `flock` prefix) and `config/build/pull/up/run` (`compose_cmd_wrapper`) with the homelab lock; **manual `down` is not covered**.

**STOP:** missing restore evidence, unexpected volume/project changes, an unsafe host-preparation hook, duplicate Sync ownership, missing networks or surprise exposure. Do not use a fresh empty volume as a fallback.

Web carries no Docker ingress labels; routing to `what-to-cook-web:8080` is defined only by the homelab Traefik file provider in `infra/infra-edge/traefik/dynamic` in the homelab repository. Docker-label routing is not used because labels on containers can be created or altered by anyone with Docker access. A Caddy configuration existed on the Pi for the pre-cutover edge; it is not current configuration on `main` and its live state is NOT VERIFIED. Caddy remains the live edge until an operator-approved cutover; no service in this repository exposes API or PostgreSQL.

## Configuration and application constraints

[The example](../deploy/.env.example) lists non-secret Stack variables; [Compose](../deploy/docker-compose.yml) is authoritative for required/optional values. `DATA_ROOT` is mandatory. `POSTGRES_PASSWORD` must come only from the existing homelab BWS workload wrapper, **not** Git, a committed `.env` or a Komodo Stack environment field. `BWS_ACCESS_TOKEN` belongs to the host, never the application Stack.

The API applies EF Core migrations at startup: deploy **one controlled API replica**, with a reviewed schema-recovery plan. Rotating a BWS secret alone does not update the existing PostgreSQL role password. For a password change, rotate the database role first, deploy the new secret, verify the application connects, and only then remove the old credential.

The Stack `pre_deploy` hook creates `${DATA_ROOT}/recipe-images` when absent, sets owner `1654:1654` and mode `0750`, and fails if the path is a symlink or the trusted parent is not a root-owned, non-group/other-writable directory. A failing hook blocks deployment before Compose runs. The hook performs host changes in a one-shot busybox container with no network, a read-only root filesystem, all capabilities dropped except `CHOWN`, `FOWNER` and `DAC_OVERRIDE`, and a pids limit. Its tests are in `scripts/prepare-what-to-cook/`.

## After an approved deployment

Verify real healthchecks, a **database-backed** recipe read/write, recipe-photo upload/read, Web SignalR connection, network isolation, expected existing database contents and a fresh backup/restore check. Record the actual outcome in the homelab rollout evidence.

If verification fails, stop further changes and follow the **previously rehearsed homelab recovery procedure**. This page does not claim that reverting a Git commit automatically reverses migrations or restores data. Local development uses [Aspire](../README.md#run-locally), not this production procedure.
