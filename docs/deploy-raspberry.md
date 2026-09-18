# Raspberry Pi deployment

Production deployment is owned by the homelab repository and Komodo. This repository provides the passive application Compose contract at `deploy/docker-compose.yml`; it does not own SSH deployment, BWS, Periphery, host preparation, routing, or backup automation.

## Canonical contract

See [self-hosted deployment](self-hosted-deployment.md) for the canonical variable-ownership table. The homelab Resource Sync Stack supplies only the documented non-secret variables. Its approved Periphery wrapper supplies `POSTGRES_PASSWORD` from the existing `homelab-workloads` BWS scope.

Do not commit `POSTGRES_PASSWORD`, BWS tokens or project IDs, a production `deploy/.env`, or a local deployment wrapper to this repository. The application contract does not require `BWS_ACCESS_TOKEN`; it is owned by the homelab Periphery configuration. Compose-owned internal values are derived by `deploy/docker-compose.yml`, not supplied by BWS or Stack configuration.

## Deployment boundary

The Stack must reuse the verified existing Compose project name, PostgreSQL volume, and `DATA_ROOT`. Homelab prepares `${DATA_ROOT}/recipe-images` as owner `1654:1654` with mode `0750` before deployment.

The Web service is the only service routed through the shared edge at `what-to-cook-web:8080`. PostgreSQL and API stay internal. Preserve forwarded headers and WebSocket upgrades for Blazor SignalR.

## Local development

Use Aspire locally:

```bash
dotnet run --project src/WhatToCook.AppHost
```
