# Deployed storage smoke

## ADDED Requirements

### Requirement: Disposable deployment proves persistent API storage
The integration suite SHALL create and read a recipe through the actual deployed API connected to the PostgreSQL service.

#### Scenario: Disposable recipe
- **WHEN** the isolated Compose stack is healthy
- **THEN** a recipe can be created through HTTP and read back by its ID.

### Requirement: Disposable deployment proves photo storage
The integration suite SHALL upload a representative image through the API and read the same bytes through the deployed static-file route, using the configured temporary bind mount.

#### Scenario: Image round-trip
- **WHEN** the recipe has been created
- **THEN** a supported image can be uploaded, exists in the host temporary bind mount and its bytes can be read back through the API.

### Requirement: Production exposure remains unchanged
These diagnostics SHALL use a loopback-only API host port in the ephemeral test override and SHALL NOT publish a production API port.
