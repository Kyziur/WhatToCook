# ARC-04 — household trust boundary and ownership

**Status:** current deployment design documented; **not** approval to expose the service publicly.  
**Scope:** the current trusted household LAN application. This decision describes intended boundaries, not a live firewall or recovery validation.

## Present architecture

```text
Household browser
   │ HTTPS / Blazor SignalR (via homelab edge reverse proxy)
   ▼
Web (Blazor Interactive Server; edge + internal networks)
   │ server-side HTTP client (Api__BaseUrl=http://api:8080)
   ├────► API (internal app network; not proxy-published)
   │       ├────► PostgreSQL (internal app network / named volume)
   │       ├────► Recipe images (host bind mount)
   │       └────► External supported recipe sources (egress/proxy)
   └────► media via Web image proxy → API storage route
```

**Application-owned:** endpoints and DTOs, validation, image storage paths, EF Core migrations, containers and the passive Compose contract.

**Homelab-owned:** identity/edge network access policy, reverse proxy/TLS and WebSocket upgrades, Docker shared-network membership, egress/proxy policy, Komodo, Periphery, BWS injection, host mounts, backups/restore and monitoring. Follow [the deployment runbook](../self-hosted-deployment.md) for these operational controls; **do not duplicate** secret or backup procedures here.

## Current security assumptions and constraints

| Boundary | Current contract | Consequence |
| --- | --- | --- |
| Browser → Web | Only Web is intended to be reachable through the household edge; Web uses Blazor Interactive Server and SignalR | A browser connection does not itself establish application-level user authorization |
| Web → API | API is called server-side through typed/named HttpClients; the Compose network is not a user identity boundary | API routes currently have no user-scoped authorization; a network-path mistake could expose recipe writes and image uploads |
| API → PostgreSQL/photos | API has database connection string and a dedicated image bind mount; DB has no published port | Secrets and file permissions must remain restricted; database or image health is not proved by Web liveness |
| API → remote imports | Importer uses a configured HTTP client and optional egress proxy | Before untrusted requests are allowed, confirm effective DNS/proxy routing and SSRF policy (SEC-03) |
| Operational controls | Homelab supplies routing, identity edge, credentials and recovery | Compose config/PR CI is not evidence of deployed isolation, credentials, usable backups or restore |

Current `UseAntiforgery` in the Web project protects its configured form flow; it is **not** a substitute for authenticating/authorizing the API. A reverse proxy, Tailscale or trusted LAN rule may restrict *reachability*, but does not define which person is allowed to alter a recipe.

## Guardrail before any scope expansion

**No public/Web API exposure, additional less-trusted households, or multi-user sync** should be approved until these choices are explicit:

1. **Users/data:** household-only shared catalog vs. per-user/per-household tenancy; who can view, edit, delete or import recipes and upload images.
2. **Authentication:** select an identity provider if required; decide browser session/cookie handling, service-to-service access from Web to API and where authorization is enforced. Do not assume the Web server's identity equals an authenticated end user.
3. **Network:** map every edge→Web, Web→API, API→database and API→egress route, including remote-VPN/Tailscale connectivity; deny direct API/database ingress unless deliberately protected.
4. **HTTP and media:** inventory every API endpoint, OpenAPI route, static image route and importer path; review CSRF, authorization, CORS, rate limits and data isolation for the chosen threat model.
5. **Operation:** verify effective firewall/network routing, host permissions, backup **and isolated restore**, secret injection and OTLP handling in the actual homelab before release.

### Verification and ownership

- Repository CI checks the **rendered Compose contract** (CI-07); it does not test the live host firewall, identity provider, proxy, backup restore or true user authorization.
- The homelab operator must supply network reachability proofs, deployment mount write/read checks and restore evidence (SEC-01/02, OPS-02/04/06/07, CI-04).
- The application owner must define and HTTP-test authorizations for all affected API routes **before** enabling untrusted access. The provider choice and authorization model are still undecided; this ADR intentionally does not prescribe Keycloak, Authentik or another product.

**Source of truth:** [Compose contract](../../deploy/docker-compose.yml), [API startup](../../src/WhatToCook.Api/Program.cs), [Web startup](../../src/WhatToCook.Web/Program.cs), and [OpenSpec self-hosted platform](../../openspec/specs/self-hosted-web-platform/spec.md).
