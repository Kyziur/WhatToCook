# Self-hosted deployment (Komodo / Bitwarden Secrets Manager)

WhatToCook is deployed as an application service by the homelab repository for the household local network. The homelab repository owns Komodo, the Periphery server, reverse proxy, shared Docker networks, deployment orchestration, and backup/restore data operations. This repository owns the application images and the application Compose contract.

## 1. Deployment contract

The canonical production deployment is a Komodo Stack or Komodo Action configured in the homelab repository. The Stack may consume `deploy/docker-compose.yml` from this Git repository.

Required non-secret settings:

- `DATA_ROOT` - persistent host directory containing recipe images.
- `POSTGRES_DB` - database name, normally `whattocook`.
- `POSTGRES_USER` - PostgreSQL bootstrap/runtime user until a separate least-privilege runtime role is introduced.
- `ASPNETCORE_ENVIRONMENT=Production`.
- `SEED_DATA_ENABLED=false`.
- `DOCKER_PLATFORM` when the target architecture must be explicit.
- `OTEL_EXPORTER_OTLP_ENDPOINT`, `HTTP_PROXY`, `HTTPS_PROXY`, and `NO_PROXY` when the homelab infrastructure requires them.

`POSTGRES_PASSWORD` is required at deployment time. It must not be placed in GitHub, this repository, or a Komodo Stack `environment` block.

The application reads:

- `ConnectionStrings__whattocook-db` - API database connection string.
- `Storage__RecipeImagesPath` - API image directory.
- `Api__BaseUrl` - server-side Web-to-API URL.

The API fails during startup when no database connection string is supplied. There is no production fallback password.

## 2. Bitwarden Secrets Manager

Create a dedicated Bitwarden Secrets Manager project and a machine account with read-only access to that project. Store the PostgreSQL credential under a POSIX-compatible key such as `POSTGRES_PASSWORD`.

Install and pin the Bitwarden Secrets Manager CLI (`bws`) on the Komodo Periphery host. Store the machine access token outside Git and outside the Komodo Stack environment, for example in a root-owned file readable only by the deployment wrapper. The token is itself a secret and must be provisioned during host bootstrap.

Use a trusted host-side wrapper or a Komodo Action that executes the wrapper on Periphery:

```bash
bws run --project-id <bitwarden-project-id> -- docker compose -f /opt/stacks/whattocook/deploy/docker-compose.yml up -d --wait
```

The wrapper must:

- load `BWS_ACCESS_TOKEN` from the protected host configuration;
- scope retrieval to the WhatToCook Bitwarden project;
- invoke only the expected Compose command;
- use `umask 077` where temporary files are created;
- never echo the environment or secret values;
- fail before deployment when Bitwarden is unavailable or a required secret is missing.

Komodo's Stack `environment` is written to a host `.env` file. Do not use Komodo interpolation such as `[[DB_PASSWORD]]` for application secrets if Bitwarden is intended to remain the source of truth.

The official Komodo documentation does not document a native Bitwarden Secrets Manager provider. Verify the exact Komodo version and Action/Procedure wiring in the homelab repository before enabling automatic deployment.

## 3. GitHub and Komodo

The homelab repository should configure:

- the GitHub repository and branch used by the Stack;
- the Komodo Server/Periphery target;
- the Stack run directory;
- the Compose file path `deploy/docker-compose.yml`;
- the explicit Compose project name;
- the GitHub webhook for the selected Stack or Action;
- required external networks (`edge`, `observability`, and `egress`), if still used by the homelab;
- GHCR credentials only if the Stack consumes private prebuilt images.

Keep API and PostgreSQL on internal Docker networks. Expose only the Web service through the homelab reverse proxy. The proxy must support HTTPS and long-lived WebSocket/SignalR connections.

A public GitHub repository does not make the application public by itself. Do not expose the API, PostgreSQL, Komodo, or Periphery directly to the Internet. The API currently assumes trusted internal access and does not provide user authentication.

## 4. Compose and persistence

`deploy/docker-compose.yml` is the application deployment contract. It requires `DATA_ROOT` and `POSTGRES_PASSWORD`; it does not define production secret values.

Before creating the Komodo Stack, inspect the existing host:

```bash
docker compose ls
docker volume ls
```

The current PostgreSQL volume is logically named `postgres-data`, but the actual Docker volume name also depends on the Compose project name. Configure Komodo with the existing project name or perform an explicit volume migration. Never accept a new empty PostgreSQL volume without verifying the data.

The API image runs as a non-root user. The homelab bootstrap must create `${DATA_ROOT}/recipe-images` and grant the container UID write access before the first deployment.

PostgreSQL is not published on a host port. The database healthcheck must pass before the API starts, and the deployment smoke check must include an API database-backed request rather than only the Web liveness endpoint.

## 5. Images

The current Compose file can build the API and Web images from the GitHub checkout. This is the smallest migration and preserves the existing Dockerfiles, but it builds the .NET SDK stages on the target host.

For a later production hardening step, GitHub Actions can build and publish immutable API/Web images to GHCR. In that model:

- images are tagged with a commit SHA or release version;
- the Stack uses immutable tags or digests;
- private registry credentials stay in Komodo/Periphery configuration;
- the target does not compile application source during deployment.

## 6. Database migrations and rotation

The API currently applies EF Core migrations during startup. Keep this behavior only if the homelab deployment allows one controlled API instance to own migrations. For multiple replicas or schema-sensitive rollbacks, move migrations to an explicit pre-deploy procedure and require rollback-compatible migrations.

Changing `POSTGRES_PASSWORD` in Bitwarden does not change the password of an existing PostgreSQL role. Rotate credentials by changing the database role password first, deploy the new application secret, verify a database-backed request, and remove the old credential only after successful verification.

The current database user is also used as the PostgreSQL bootstrap user. Introduce a separate least-privilege runtime role before exposing the service beyond the trusted LAN.

## 7. Backup, restore, and rollback

Komodo replaces the old application deployment controller, but it does not automatically replace application data operations. Backup and restore are owned by the homelab repository and must stay outside the application release path.

The homelab backup procedure must produce database backups with `pg_dump` through the protected BWS wrapper (direct `docker compose` execution cannot evaluate this contract without `POSTGRES_PASSWORD`), cover `${DATA_ROOT}/recipe-images`, and keep archives outside GitHub with access control or encryption. A restore drill against a throwaway PostgreSQL container must validate the archive before it is trusted.

Before migration:

1. Create a database-and-images backup with the homelab backup procedure.
2. Record the current Compose project name and PostgreSQL volume name.
3. Verify the backup manifest and perform a restore drill.
4. Deploy the first Komodo release without deleting the old volume.
5. Verify recipes, images, `/health`, and a database-backed request.

Backups must be retained according to the homelab policy; a SHA-256 manifest alone does not provide confidentiality or authenticity.

## 8. Local development

Aspire remains the local development workflow. The Web application uses Blazor Interactive Server rendering in both local and hosted deployments:

```bash
dotnet run --project src/WhatToCook.AppHost
```

Local direct API execution now requires a supplied connection string through Aspire, environment variables, or user secrets. Missing credentials must fail instead of selecting a default password.

## 9. Verification checklist

After the first Komodo deployment verify:

- Komodo can pull the GitHub repository or the configured image;
- the BWS machine account can read only the WhatToCook project;
- deployment fails when the BWS token or `POSTGRES_PASSWORD` is unavailable;
- the existing PostgreSQL volume is reused;
- the API healthcheck reports database readiness;
- a recipe can be created and read after a restart;
- recipe image upload works with the non-root container UID;
- Web SignalR works through the reverse proxy;
- backup and restore use the same database and image storage contract;
- a failed deployment does not delete the previous release or persistent data.
