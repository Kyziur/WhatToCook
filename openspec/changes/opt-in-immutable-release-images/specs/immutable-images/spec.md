# Immutable images

## ADDED Requirements

### Requirement: Explicit immutable publication
The application repository SHALL allow an operator to manually publish API and Web multi-platform images to GHCR from the selected reviewed commit, with source SHA and per-image digest included in the job summary.

#### Scenario: Publish selected commit
- **WHEN** an authorized operator dispatches the workflow on the reviewed revision
- **THEN** both images are built for linux/amd64 and linux/arm64 and the resulting digest is recorded without automatic deployment.

### Requirement: Opt-in image deployment
The deployment overlay SHALL remove the API and Web local build instructions and require image references while preserving their existing networks, secrets, mounts and runtime hardening.

#### Scenario: No pins
- **WHEN** required API or Web image variables are absent
- **THEN** Compose validation fails.

#### Scenario: Pins present
- **WHEN** both image references are provided
- **THEN** Compose renders images with no local build instructions, and neither API nor PostgreSQL receives a published host port.

### Requirement: Existing production deployment remains unchanged
The base Compose file and the homelab Stack SHALL NOT switch to the immutable overlay automatically. Promotion and rollback remain separately reviewed, homelab-owned operations.
