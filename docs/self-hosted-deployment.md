# Self-hosted deployment (Komodo / Bitwarden Secrets Manager)

WhatToCook is an application service for the household local network. The homelab repository owns Bitwarden Secrets Manager (BWS), Komodo, Periphery, host-path preparation, shared Docker networks, routing, and backups. This repository owns the application images and the passive Compose contract in `deploy/docker-compose.yml`.

## Deployment variable contract

The canonical production deployment is a homelab-managed Komodo Resource Sync Stack consuming this repository's `deploy/docker-compose.yml`. This repository MUST NOT contain BWS tokens or project IDs, a `bws run` invocation, a Komodo hook, a host-preparation script, or a production `deploy/.env` file.

| Owner | Variables | Contract |
| --- | --- | --- |
| BWS workload secret | `POSTGRES_PASSWORD` | The existing `homelab-workloads` BWS scope supplies it only to the Periphery-wrapped Compose process. It is never committed and never declared in a Stack `environment` block. |
| Required non-secret Stack variables | `DATA_ROOT`, `POSTGRES_DB=whattocook`, `POSTGRES_USER=whattocook`, `ASPNETCORE_ENVIRONMENT=Production`, `SEED_DATA_ENABLED=false`, `DOCKER_PLATFORM` | Resource Sync generates the Stack `.env` from these values. |
| Optional homelab overrides | `OTEL_EXPORTER_OTLP_ENDPOINT`, `HTTP_PROXY`, `HTTPS_PROXY`, `NO_PROXY` | Omit an override where the Compose default already matches homelab infrastructure. |
| Compose-owned internals | `ConnectionStrings__whattocook-db`, `Storage__RecipeImagesPath`, `Api__BaseUrl` | Compose derives these values; they are not Stack or BWS inputs. |

The application contract does not require `BWS_ACCESS_TOKEN`. The Periphery host owns that token and the approved wrapper; the generated Stack `.env` contains non-secret values only.

`POSTGRES_PASSWORD` and `DATA_ROOT` are fail-closed Compose inputs. The API has no production fallback password.

## Komodo and network boundary

The homelab Stack configuration selects the canonical Git repository and branch, an existing Periphery target, the `deploy` run directory, and the existing Compose project name. Before the first Stack is created, inspect the live host and reuse its Compose project name, PostgreSQL volume, and `DATA_ROOT`; never accept an inferred new empty PostgreSQL volume.

Keep API and PostgreSQL on internal Docker networks. Expose only the Web service through the shared homelab edge at `what-to-cook-web:8080`. The reverse proxy must preserve forwarded headers and WebSocket upgrades for Blazor SignalR. Do not publish host ports or proxy API or PostgreSQL directly.

## Compose and persistence

`deploy/docker-compose.yml` is the application deployment contract. It requires `DATA_ROOT` and `POSTGRES_PASSWORD`; it does not define production secret values.

The PostgreSQL volume is logically named `postgres-data`, but its Docker name depends on the Compose project name. Configure Komodo with the verified existing project name or perform an explicit volume migration. Never create a parallel empty database by inference.

The API image runs as `1654:1654`. Before deployment, homelab MUST create `${DATA_ROOT}/recipe-images`, set its owner to `1654:1654`, and set mode `0750`. This makes the host bind mount writable by the container without granting root access.

PostgreSQL is not published on a host port. Its healthcheck must pass before the API starts; the API healthcheck must pass before the Web service starts. The deployment smoke check must include a database-backed API request, not only Web liveness.

## Images and migrations

The current Compose file builds API and Web images from the Git checkout. A future immutable-image publication workflow is separate work.

The API applies EF Core migrations at startup. Deploy exactly one controlled API replica. Multi-replica migration orchestration is separate work.

Changing `POSTGRES_PASSWORD` in BWS does not change the password of an existing PostgreSQL role. Rotate the database role first, deploy the replacement secret, verify a database-backed request, then remove the old credential.

## Backup, restore, and rollback

Backup and restore are homelab-owned and remain outside the application release path. The homelab backup action selects PostgreSQL by Compose labels and invokes `docker exec pg_dump`; it does not run Compose or require BWS. Backup coverage includes the verified Stack root and `${DATA_ROOT}/recipe-images`.

Before the first Komodo deployment:

1. Create a database-and-images backup with the homelab procedure.
2. Record the verified Compose project name and PostgreSQL volume name.
3. Validate the backup manifest and perform a restore drill.
4. Deploy without deleting the existing volume.
5. Verify recipes, images, `/health`, and a database-backed request after API and Web restart.

## Local development

Aspire remains the local development workflow. The Web application uses Blazor Interactive Server rendering in both local and hosted deployments:

```bash
dotnet run --project src/WhatToCook.AppHost
```

Local direct API execution requires a connection string through Aspire, environment variables, or user secrets. Missing credentials fail rather than selecting a default password.

## Verification checklist

After the first homelab deployment, verify that the wrapped `config`, `pull`, and `up` lifecycle succeeds; all three services pass healthchecks; only non-secret values appear in the generated Stack `.env`; the Web route keeps a SignalR connection; and API/PostgreSQL are not proxy-addressable. Run the existing backup actions, validate the gzip dump and captured recipe-image data, and complete a restore drill before production readiness.
