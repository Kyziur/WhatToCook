# WhatToCook

WhatToCook is a self-hosted recipe catalog, meal planner, and shopping list for a household LAN.

## Local development

Requires the .NET 10 SDK selected by `global.json`, Docker Desktop for PostgreSQL-backed integration tests, and Node.js for Playwright. Restore and build with `dotnet restore` and `dotnet build WhatToCook.slnx`. Run the local Aspire host with `dotnet run --project src/WhatToCook.AppHost`.

## Production deployment

Production deployment is owned by the homelab repository and Komodo. This repository provides the application Compose contract in `deploy/docker-compose.yml`; it does not contain production secret values or the Komodo configuration.

Bitwarden Secrets Manager supplies deployment secrets through a trusted host-side wrapper or Komodo Action. Do not put secret values in GitHub or in a Komodo Stack `environment` block.

See [`docs/self-hosted-deployment.md`](docs/self-hosted-deployment.md) for the Komodo/Bitwarden contract, persistence requirements, backup and restore, rotation, and verification checklist. Raspberry Pi-specific migration notes are in [`docs/deploy-raspberry.md`](docs/deploy-raspberry.md).
