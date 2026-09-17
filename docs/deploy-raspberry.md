# Raspberry Pi deployment

Production deployment of WhatToCook is owned by the homelab repository and Komodo. This repository provides the application Compose contract at `deploy/docker-compose.yml`; it no longer owns the SSH deployment controller.

## Komodo contract

Configure the Komodo Stack or Action in the homelab repository with:

- this GitHub repository and the deployment branch;
- a Periphery server running on the Raspberry Pi;
- `deploy/docker-compose.yml` as the Compose file;
- a stable Compose project name matching the existing PostgreSQL deployment;
- a stable `DATA_ROOT` for recipe images; PostgreSQL persists in the named `postgres-data` volume;
- all external Docker networks declared by the Compose contract: `edge`, `observability`, and `egress`;
- the reverse proxy route to `what-to-cook-web:8080`.

The Web service is the only application service that should be exposed through the homelab reverse proxy. PostgreSQL and the API remain internal Docker services.

## Bitwarden Secrets Manager

The homelab deployment must provide `POSTGRES_PASSWORD` at deployment time from Bitwarden Secrets Manager. Do not commit it to GitHub or put its value in the Komodo Stack environment, because Komodo writes that environment to a host `.env` file.

Use a dedicated Bitwarden project and a read-only machine account. Keep the `BWS_ACCESS_TOKEN` in a protected Periphery/host configuration and run the trusted deployment wrapper through Komodo Action/Procedure:

```bash
bws run --project-id <bitwarden-project-id> -- docker compose -f /opt/stacks/whattocook/deploy/docker-compose.yml up -d --wait
```

The wrapper must fail when Bitwarden or a required secret is unavailable and must not print the environment or secret values. Komodo currently has no documented native Bitwarden provider, so the homelab repository must verify the Action/Procedure integration for the installed Komodo version.

## First migration

Before creating or deploying the Komodo Stack:

1. Back up PostgreSQL and recipe images.
2. Record `docker compose ls` and `docker volume ls` on the Raspberry Pi.
3. Configure Komodo with the existing Compose project name, or perform an explicit volume migration.
4. Pre-create `DATA_ROOT/recipe-images` with write access for the API container UID.
5. Verify the required external Docker networks.
6. Deploy without removing the old PostgreSQL volume.
7. Verify `/health`, a database-backed recipe request, image upload, restart persistence, and rollback behavior.

The logical volume is `postgres-data`, but Docker prefixes it with the Compose project name. A changed Komodo Stack name can silently create a new empty database.

## Data operations

Backup and restore are data operations owned by the homelab repository, not by this codebase. The homelab backup procedure must provide them under the same contract:

- produce database backups with `pg_dump` through the protected BWS wrapper; direct `docker compose` execution cannot evaluate this Compose contract without `POSTGRES_PASSWORD`;
- cover the database and the `${DATA_ROOT}/recipe-images` directory;
- keep archives outside GitHub, access-controlled or encrypted, with retention per homelab policy;
- validate the archive with a restore drill against a throwaway PostgreSQL container before it is trusted.

See [`docs/self-hosted-deployment.md`](self-hosted-deployment.md) for the complete application contract and verification checklist.
