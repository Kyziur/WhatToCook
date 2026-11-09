# Optional immutable WhatToCook image release

The canonical homelab Stack currently builds images from source. **Do not**
change its Compose file list, project name, volume or data root as part of
publishing. Immutable release is opt-in and remains disabled by default.

## Publish

From GitHub Actions, manually run `Publish immutable WhatToCook images` on a
reviewed commit. It publishes `ghcr.io/kyziur/whattocook-api` and
`ghcr.io/kyziur/whattocook-web` for ARM64 and AMD64, using a commit-SHA
label and outputting their distinct, immutable digests to the job summary.
Inspect provenance/SBOM and the actual registry manifests before promotion.
Keep registry permissions limited to the authorized deployment account.

## Dry-run render (no deployment)

```bash
export DATA_ROOT=/srv/whattocook
export POSTGRES_PASSWORD=validation-placeholder
export WHATTOCOOK_API_IMAGE=ghcr.io/kyziur/whattocook-api@sha256:<verified-api-index-digest>
export WHATTOCOOK_WEB_IMAGE=ghcr.io/kyziur/whattocook-web@sha256:<verified-web-index-digest>
docker compose -f deploy/docker-compose.yml -f deploy/docker-compose.immutable.yml \
  --project-name whattocook config --format json
```

This only renders configuration; the placeholder password is for the **offline
dry run**, not for deployment. Never set production secrets this way. Verify
that API and Web have image references but no `build`, PostgreSQL still
mounts `whattocook_postgres-data`, and only Web belongs to `edge`.

## Controlled promotion and rollback

A future homelab PR must explicitly add the overlay to Komodo `file_paths`,
inject **both** index digest references as non-secret Stack values, verify
availability from the Pi, and retain `project_name = whattocook` and
`DATA_ROOT=/srv/whattocook`. Run the predeploy backup/recovery and volume
identity checks first, then a recipe/image smoke after the change.

Rollback selects the previously **verified API and Web digests** with compatible
database migrations. Pinning images does not reverse a destructive EF Core
migration. If no proven restore and rollback plan exists, do not promote.
