# OPS-05 — Opt-in immutable image publishing and promotion

## Why
The current Komodo Stack builds API and Web from the Git checkout. Mutable base image tags can produce different artifacts from the same source revision and complicate deterministic rollback.

## Scope
Provide a manual GitHub Actions multi-architecture GHCR publish workflow and a separate optional Compose overlay that removes both API/Web local builds and requires externally supplied image references. Test the rendered overlay for absence of build, persistence of existing mounts and network exposure. Existing Komodo Resource Sync and production Compose remain unchanged.

## Deployment decision
Promotion to Komodo requires a separate homelab-controlled PR after verifying registry access, both ARM64+AMD64 manifests, SBOM/provenance, correct SHA source, digest pins, backup/restore and rollback. Do not auto-enable this overlay in production, alter existing named volumes, or publish an unreviewed release.
