# Security, deployment, and observability

- **Audit snapshot:** [`4eb64d7e3de01faa7bef32db74da94e7e1c50913`](https://github.com/Kyziur/WhatToCook/tree/4eb64d7e3de01faa7bef32db74da94e7e1c50913), originally audited 2026-11-02.
- **Assessment:** the findings and recommendations were reviewed against repository code, configuration, tests, and deployment documentation on 2026-11-06. The snapshot remains the evidence boundary for the source observations below.
- **Status:** this is an action plan, not evidence that a mitigation, deployment, backup, restore, alert, or security scan has completed. Static code patterns do not establish a production vulnerability or incident.
- **Related chapter:** [repository audit and action plan](2026-11-02-repository-audit.md).

## Scope, priority, and operating boundaries

The original review was predominantly static. It did not establish live network reachability, the target host's ACLs or edge policy, the deployed authentication model, whether uploaded photos contain EXIF data, whether the OTLP collector receives telemetry, or whether a backup can be restored. This assessment did not run a production deployment, browser/CSRF exercise, security scan, backup, or restore drill. Treat each runtime item below as a verification task until its stated acceptance evidence exists. Do not describe any of these findings as a confirmed exploit, outage, data loss, or CVE.

- **P1** means verify or complete before the first production rollout where stated. A P1 may be an operational gate owned by the homelab rather than an application-code change.
- **P2** means address after P1, or when the stated exposure, privacy, release, or diagnostic condition applies.
- **P3 / optional** means act only when the stated trigger or measurement justifies the cost. Each optional item includes a trigger, proposed action, and way to verify value.
- **Verify first** means repository evidence is insufficient to establish the target environment's state.

### Ownership and deployment invariants

The application repository owns application code and the passive Compose contract. The homelab owns Komodo/Periphery, BWS, host-path preparation, shared Docker networks, routing/edge policy, and backup/restore. Keep that boundary intact:

- The recorded deployment identity is Komodo Resource Sync stack `what-to-cook`, Compose project `whattocook`, with data root `/srv/whattocook`. Verify the live target before rollout or volume operations; do not silently change the project identity.
- Keep `POSTGRES_PASSWORD` in the existing `homelab-workloads` BWS scope and inject it only through the approved Periphery wrapper. The generated Stack `.env` contains non-secret values only. The application does **not** require `BWS_ACCESS_TOKEN`; do not add a BWS token, BWS invocation, Komodo hook, production `.env`, or host-preparation script to this repository.
- Preserve the existing PostgreSQL volume. Compose's logical volume name `postgres-data` is project-prefixed by Docker; changing the Compose project can select a different, empty volume. Confirm the live project and volume rather than treating the logical name or a zero-row database as sufficient evidence of identity. A genuinely fresh installation may intentionally be empty.
- Before deployment, the homelab must prepare `/srv/whattocook/recipe-images` for the API's `1654:1654` identity with mode `0750`, using its fail-closed Periphery host-preparation procedure. Do not loosen the permissions to `0777` or move host ownership into an application startup hook.
- Keep PostgreSQL and the API off the public edge. The documented route is edge to Web; Web uses the internal API. Do not proxy or publish the API or PostgreSQL directly. Network configuration is not evidence of actual reachability or isolation.

These boundaries and invariants are recorded in the [self-hosted deployment contract](../self-hosted-deployment.md#L3-L34) and its [backup/restore and verification checklist](../self-hosted-deployment.md#L44-L68). Compose defines the `postgres-data` volume and the service network memberships in [`deploy/docker-compose.yml`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L32-L34) and [its network section](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L122-L143).

## Security findings

### SEC-01 — API authorization depends on the verified access boundary

**Priority:** P1 before rollout. **Owner:** homelab for exposure/edge policy; application owner if the agreed trust model requires application authorization.

**Finding and rationale:** The reviewed API startup maps recipe, import, plan, and shopping endpoints without an evident authentication/authorization policy. This is a static application finding, not proof that an unauthorized party can reach the service. The edge may have controls outside this repository, but edge protection of Web alone does not establish that direct API access or static photo URLs cannot bypass it. A caller that can reach the unauthenticated API could invoke its read/write operations.

**Action:** Write down the intended trusted users and network segments. Verify access to both Web and API, including the `/recipe-images` static-file route, from trusted clients, guest/IoT or otherwise untrusted segments, and relevant containers. If access extends beyond the trusted household boundary, design identity and authorization for the complete path: Web, API mutations and reads, photos, and any path that bypasses the edge. An edge identity gate is sufficient only if the API and its photos cannot be reached around it. Keep the existing LAN-only model only if the tested network boundary actually enforces it.

**Evidence:** [`Program.cs` maps API endpoints and OpenAPI without visible auth setup](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L14-L65); representative [recipe mutation routes](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Editor/EditorEndpoints.cs#L11-L24); [deployment edge/API contract](../self-hosted-deployment.md#L20-L24).

**Verification and trade-offs:** Record a reachability matrix for trusted and untrusted sources against Web, direct API, `/recipe-images`, and OpenAPI. Expected result is successful access only from authorized sources, with denied paths unreachable or returning 401/403. If the service is intentionally restricted to a trusted LAN, network isolation is simpler than adding identity features, but the boundary must be tested and monitored. If broader access is required, application/API enforcement prevents a network-path bypass but adds identity lifecycle and authorization work.

### SEC-02 — Validate API membership in shared networks; membership is not exposure proof

**Priority:** P1 before rollout. **Owner:** homelab.

**Finding and rationale:** Compose attaches API to the internal default network and external `observability` and `egress` networks; Web is attached to default, edge, and observability. This creates configured shared-network paths worth reviewing. It does not prove that any container can reach the API in the live deployment, that an external network is internet-facing, or that the API is publicly exposed. Conversely, `default: internal` does not isolate API from its other attached networks.

**Action:** Review live Docker network membership, host firewall/ACLs, proxy routes, and allowed service directions. Preserve the intended paths (edge to Web; Web to API; API to PostgreSQL; API to approved egress proxy and telemetry collector) while denying unneeded access to API from unrelated peers. Coordinate any network separation with the collector and proxy owners so required OTLP and import egress continue to work.

**Evidence:** [Compose API and Web network attachments](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L82-L85) and [declared internal/external networks](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L122-L140).

**Verification and trade-offs:** Test the intended connection matrix from the actual target: edge to Web succeeds; Web to API succeeds; API to PostgreSQL, approved proxy, and collector succeeds; unrelated peers and untrusted segments cannot reach API or PostgreSQL. Confirm the negative cases at the socket/HTTP layer, not just by reading Compose. Restricting membership may improve isolation but can break telemetry/imports; document each necessary flow before removing a network.

### SEC-03 — Resolve importer DNS/proxy TOCTOU risk if egress exposure warrants it

**Priority:** P2, conditional on API exposure, DNS behavior, and egress controls. **Owner:** application and homelab jointly.

**Finding and rationale:** The importer resolves and checks the hostname before making the HTTP request. The HTTP client later performs connection resolution; a proxy may resolve the destination separately. The reviewed address filter covers several private/local ranges, but that check and the eventual socket destination are separate operations and the manual special-address list may not cover every relevant IPv4/IPv6 case. This is a construction risk, not a demonstrated exploit. Risk is reduced by the strict supported-source adapters, HTTP(S)-only handling, response limits, timeout, and disabled redirects.

**Action:** First trace the actual request path, including whether production uses an HTTP(S) proxy and where DNS is resolved. If a private destination must be impossible, either bind the validated address to the connection while preserving the requested host for HTTP/TLS, or enforce deny-private/metadata egress at a network/proxy layer that sees the effective destination. Do not rely solely on a preflight DNS lookup if the connection or proxy performs a fresh lookup.

**Evidence:** [DNS validation and later HTTP request](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportPageFetcher.cs#L19-L39), [address filtering](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportPageFetcher.cs#L82-L125), and [adapter selection](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportSourceAdapterBase.cs#L14-L23).

**Verification and trade-offs:** In an isolated test, use controlled DNS answers/rebinding and special IPv4/IPv6 targets, then assert the effective socket or proxy destination never reaches loopback, private, link-local, or metadata addresses. Also verify a valid supported public source still imports and redirects remain rejected. Connection pinning is more code and must preserve HTTPS host validation; egress enforcement is simpler to centralize but depends on correct proxy/network policy.

### SEC-04 — Reassess CSRF protection when browser credentials or exposure change

**Priority:** P2 before authentication or browser exposure changes; verify sooner if a browser can directly reach the API. **Owner:** application; homelab for network and edge policy.

**Finding and rationale:** The photo upload endpoint explicitly disables antiforgery. The reviewed application startup does not show cookie authentication, so this is not by itself proof of an exploitable classic CSRF path. However, an unauthenticated API that trusts network reachability can also receive cross-site requests from a browser on that network; cookies are not required for the browser to send a request. The actual attack path depends on browser reachability, request shape, and edge/network controls. Cookie/session or edge-based credentials add further ambient authority.

**Action:** Keep the reason for `.DisableAntiforgery()` explicit. Map which mutation routes a browser on a trusted network can reach and which credentials, if any, accompany cross-site requests. If browser access is allowed, choose and enforce suitable cross-origin and CSRF protections for state-changing routes, including upload. Use bearer credentials only where they are not ambiently attached and the full request path enforces them; a server-to-server-only design also requires verifying that browsers cannot bypass that boundary.

**Evidence:** [Upload route antiforgery exception](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Editor/EditorEndpoints.cs#L19-L24); [API startup](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L14-L27).

**Verification and trade-offs:** Under the actual network and authentication configuration, exercise negative cross-origin form/multipart requests against photo upload and other mutations from a browser on an otherwise trusted network, both without cookies and with a logged-in session if one exists. Use an isolated environment and disposable test records so a failed protection cannot alter household data. Confirm unauthorized cross-site mutations fail and legitimate same-origin upload still works. If relying on a server-to-server-only or non-ambient bearer model, prove the network and credential enforcement rather than assuming the absence of cookies is sufficient.

### SEC-05 — Strip photo metadata if the privacy/access policy requires it

**Priority:** P2 when photos may leave the trusted group or privacy policy prohibits embedded location. **Owner:** application.

**Finding and rationale:** Image storage validates size, signature, format, and pixel dimensions, then stores the original upload bytes. That code does not establish that any current photo contains EXIF/GPS, or that anyone has accessed such data. A photo URL may expose embedded metadata to anyone authorized to retrieve the file.

**Action:** Decide whether published recipe images may retain EXIF, GPS, IPTC, or XMP metadata. If policy requires removal, sanitize or re-encode on ingest and ensure the bytes served by `/recipe-images` are the sanitized result. Select a method that preserves the supported formats and desired image quality/animation rather than silently dropping useful image behavior.

**Evidence:** [Original bytes are copied to filesystem storage](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Storage/FileSystemRecipeImageStorage.cs#L19-L70); [photos are served as static files](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L36-L47).

**Verification and trade-offs:** In an isolated test environment, upload a known test image with GPS EXIF to a disposable recipe, retrieve the served file, and inspect it to confirm prohibited tags are absent while image dimensions and supported rendering remain correct. Test representative formats, including any format whose animation/metadata behavior matters. Re-encoding adds CPU and may change quality or animation; if images remain strictly inside a trusted group and policy permits metadata, document that decision rather than treating removal as a confirmed leak response.

### SEC-06 — Apply abuse limits in proportion to the actual exposure

**Priority:** P2 if less-trusted clients can reach the API; otherwise verify existing edge limits and defer endpoint changes. **Owner:** homelab for edge limits; application for endpoint concurrency where needed.

**Finding and rationale:** The reviewed API setup does not demonstrate request-rate or concurrency limits. The importer has a 10-second timeout and 1 MB response cap, and uploads have a 5 MB body/image bound; these bound individual work but not request volume. The edge or proxy may already enforce appropriate limits, which this repository cannot establish.

**Action:** Inspect existing edge/proxy policy first. If it is insufficient for the exposed audience, set budgets for request rate and concurrency, prioritizing URL import and image upload because they consume outbound connections, CPU, memory, and disk. Return a controlled 429 response where appropriate and preserve cancellation; avoid implementing duplicate limits at multiple layers without a clear ownership model.

**Evidence:** [API configuration and importer timeout](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L17-L27), [import request routes](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/ImportEndpoints.cs#L7-L40), and [image size limits](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Storage/FileSystemRecipeImageStorage.cs#L12-L23).

**Verification and trade-offs:** Agree on a request/concurrency budget with the operator, exercise a controlled burst (including the agreed 100-request test if appropriate), and verify excess work is rejected or queued without unbounded CPU, memory, connections, or disk growth. Confirm normal household use and importer cancellation still work. Rate limits may reject legitimate bursts; tune to measurements rather than adopting an arbitrary threshold.

### SEC-07 — Restrict OpenAPI only if schema visibility is outside the trust model

**Priority:** P3 / optional. **Trigger:** the API schema is reachable from a segment where endpoint discovery is not intended, or policy requires reducing public metadata.

**Action:** If triggered, expose OpenAPI only in development or behind the already-approved trusted access boundary. Do not treat hiding the schema as authorization; apply access controls to the endpoints independently.

**Evidence:** [`Program.cs` maps OpenAPI unconditionally in the reviewed startup](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L48-L50).

**Verification and trade-offs:** Request `/openapi/v1.json` from trusted and untrusted segments and confirm the intended policy, while separately proving that API routes themselves remain denied to unauthorized clients. Restricting the schema can reduce casual discovery but does not block a caller who already knows an endpoint; keep it available for trusted development if useful.

### SEC-08 — Make security scan results and failure conditions observable

**Priority:** P2 after the CI command/exit-code issue is addressed. **Owner:** application/CI.

**Finding and rationale:** The workflow contains a dependency-audit step, but the command text alone does not prove that each scan ran successfully, that its result was retained, or that a finding fails the pipeline. No scan output or CVE finding was established by this review.

**Action:** After CI-02 is corrected, verify the effective behavior of dependency scanning and any existing secret-scanning/SAST/SBOM controls. For each control, define its policy, preserve results as useful CI output/artifacts, and ensure failures cannot be hidden by a later successful command. Add a new scanner only when current GitHub or homelab controls do not already meet the agreed requirement.

**Evidence:** [Dependency audit workflow step](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/.github/workflows/pr-ci.yml#L46-L48).

**Verification and trade-offs:** On a disposable branch or controlled fixture, demonstrate a clean report and a deliberately failing scan/policy result that makes CI fail; verify the report is retained and the gate's behavior is documented. Use a harmless test secret fixture that cannot become a real credential. Avoid declaring CVEs, scanner absence, or clean status without actual results. Additional scanning improves visibility but costs CI time and maintenance; avoid duplicate tools without a coverage gap.

## Deployment and operations findings

### OPS-01 — Keep startup migrations single-replica and rollback-aware

**Priority:** P2; required before a schema-changing release, with a separate migration job conditional on deployment needs. **Owner:** application and homelab.

**Finding and rationale:** The API runs EF Core migrations during startup. The current deployment contract calls for one controlled API replica. A migration can delay or prevent startup, and rolling back application code after an incompatible/destructive schema migration does not restore the prior schema.

**Action:** Before each schema-changing deployment, take and validate the database backup, review migration compatibility in both directions, and ensure only one API instance performs startup migration. Use expand/contract migrations where old and new code may overlap. Introduce a separate single-runner migration/pre-deploy job only when controlled rollout, multiple replicas, or a measured operational need requires it; do not add orchestration without that trigger.

**Evidence:** [`Program.cs` awaits database initialization before mapping the API](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L52-L57); [initializer applies migrations](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbInitializer.cs#L7-L23); [single-replica deployment contract](../self-hosted-deployment.md#L36-L42).

**Verification and trade-offs:** On an isolated restored copy, exercise schema N to N+1, start the intended image, then verify the prior image is compatible with the resulting schema or perform the documented database restore path. Confirm the release process cannot launch concurrent migrators. A separate migration job is more controlled but adds release orchestration; the current single-replica arrangement remains simpler if its limits are respected.

### OPS-02 — Prove PostgreSQL and image backup restoration before rollout

**Priority:** P1 before first production rollout. **Owner:** homelab.

**Finding and rationale:** Backup and restore belong to the homelab, not the application release. The deployment document describes the backup action and required coverage; that documentation does not prove a backup has run, that it includes the correct data, or that it can be restored. No RPO/RTO or successful restore was established by this repository review.

**Action:** Use the existing homelab backup procedure to capture PostgreSQL and `/srv/whattocook/recipe-images` as a coordinated recovery set. Validate the manifest and checksums, retention, and the selected Compose project/volume; perform a restore in an isolated environment before production. Record the time and measured RTO and agree an RPO with the data owner. Keep this operation in the homelab; do not put BWS access or backup credentials into the application.

**Evidence:** [Homelab ownership and documented backup action](../self-hosted-deployment.md#L3-L18) and [backup/restore steps](../self-hosted-deployment.md#L44-L54).

**Verification and trade-offs:** Restore the dump and image set into an isolated copy, start the application with the intended compatible image, verify a database-backed recipe request, retrieve representative restored photos, compare expected record/file counts or checksums, and record elapsed restore time. Do not test by overwriting production. Database and image backup cost storage and restore time; retaining them as one coherent recovery point avoids recipes referencing missing files.

### OPS-03 — Preserve Compose project and PostgreSQL volume identity

**Priority:** P1 before any rollout or change to the Komodo Stack. **Owner:** homelab.

**Finding and rationale:** The logical Compose volume is `postgres-data`, but Docker prefixes its concrete volume name with the Compose project. A changed project name can start a new empty database and look like data loss. This is a continuity hazard, not evidence that the current volume was changed or data was lost.

**Action:** Verify the existing Komodo stack/project, Docker Compose project name, actual PostgreSQL volume, and `DATA_ROOT` on the live host. Preserve the current Stack `what-to-cook`, Compose project `whattocook`, existing PostgreSQL volume, and `/srv/whattocook` data root. If a move is intended, make it an explicit, backed-up volume/data migration with a rollback plan; never infer a new volume from a Compose render or create one beside the existing data.

**Evidence:** [Project/volume continuity warning](../self-hosted-deployment.md#L20-L30); [logical volume declaration](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L32-L34) and [Compose volume name](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L142-L143).

**Verification and trade-offs:** Before deployment, record the live project and volume identifiers and compare them to the planned Stack configuration. After restart, verify existing recipe data and images remain visible and a database-backed request succeeds. Do not make “database has rows” an unconditional precondition: an intentionally fresh install can be empty. Identity comes from matching the verified project/volume and backup, not row count alone.

### OPS-04 — Prepare and exercise the image bind mount as UID/GID 1654

**Priority:** P1 before rollout. **Owner:** homelab for host preparation; application owner for upload/read smoke behavior.

**Finding and rationale:** API runs as `1654:1654` and uses a host bind mount for recipe images. A container may start and serve Web/API health while the mounted host directory is not writable. The existing container/integration checks do not prove that the real host directory has the required ownership or that a stored image can be read back through the deployed route.

**Action:** Using the fail-closed Periphery host-preparation procedure, create `/srv/whattocook/recipe-images`, set owner `1654:1654`, and mode `0750`. Keep ownership and host setup homelab-owned. Do not use `0777`, write files as root and assume the API can access them, or equate container startup with a writable mount.

**Evidence:** [Host path, identity, and permission contract](../self-hosted-deployment.md#L26-L34); [API Dockerfile runtime identity](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Dockerfile#L20-L24); [bind mount and healthcheck](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L63-L81). The existing [storage integration test](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/SelfHostedWebPlatformSpecificationTests.cs#L63-L92) checks a test storage path, not the production host mount.

**Verification and trade-offs:** On the target, verify the host path's owner/mode and effective container identity. Exercise a create/read/delete probe using only a unique file created by that probe. For API upload, use an isolated test environment where possible; if a target-host smoke is required, first create a dedicated disposable recipe, upload only to it, GET the returned image, compare the bytes, and run a database-backed request. Uploading to an existing recipe can replace and delete its previous image. Archive the test recipe and retain its referenced image, or use an approved cleanup procedure that removes the database reference before removing the file; never delete a still-referenced image. Keep the directory at `0750`.

### OPS-05 — Adopt immutable image references only when reproducible release/rollback is required

**Priority:** P2, conditional on deterministic promotion or rollback requirements. **Owner:** application and homelab.

**Finding and rationale:** Compose currently builds API and Web images from the checkout at deployment. Mutable base-image tags can change the resulting image across builds of the same source revision, so a source SHA alone may not reproduce a prior release. This is not evidence that a current image is compromised or unreproducible in practice.

**Action:** If reproducible releases or reliable rollback become requirements, agree a build/publish/promote contract with the homelab: build and scan in CI, publish immutable digests with provenance/SBOM as required, and configure Komodo to deploy a selected digest. Treat this as a deployment-contract change; do not unilaterally replace the existing build-from-checkout workflow or change the homelab's image ownership.

**Evidence:** [Current build-from-checkout deployment contract](../self-hosted-deployment.md#L36-L40); [Compose build contexts](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L40-L45) and [Web build context](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L87-L92).

**Verification and trade-offs:** Deploy the same promoted digest twice and confirm image identity is unchanged; then roll back to the previously recorded digest and verify the app/schema compatibility. Digest promotion improves repeatability but requires registry, provenance, retention, and homelab workflow changes; retain the current model until that operational value is agreed.

### OPS-06 — Verify required external networks and every allowed direction

**Priority:** P1 before rollout. **Owner:** homelab.

**Finding and rationale:** Compose expects external `edge`, `observability`, and `egress` networks. Their existence, DNS behavior, proxy/collector availability, ACLs, and actual connectivity are homelab properties not established by the Compose file. API membership in `egress` or `observability` is configured network membership, not proof of a live route or public exposure.

**Action:** Preflight the existing shared networks and their intended service names, then validate only the required paths: edge to `what-to-cook-web:8080`, Web to API, API to PostgreSQL, API to approved egress proxy for imports, and API to the configured OTLP collector. Confirm the API and PostgreSQL have no public host ports or edge route. Document the expected failure behavior when proxy or collector is unavailable; do not loosen boundaries just to make Compose start.

**Evidence:** [External network declarations and service memberships](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L68-L85) and [edge/observability/egress definitions](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L122-L140); [documented edge-only route](../self-hosted-deployment.md#L20-L24).

**Verification and trade-offs:** On the target host, verify network existence and service DNS, run positive and negative connection tests for the direction matrix, test an allowed importer request through the intended proxy, and confirm the collector route separately. Repeat the unauthorized reachability checks in SEC-01/02. Removing unnecessary network membership reduces lateral access but can break required imports or telemetry; approve flows with their owning homelab service.

### OPS-07 — Make backup, volume identity, and rollback part of the release gate

**Priority:** P1 before first rollout; repeat for releases whose migration or data risk requires it. **Owner:** homelab, with application compatibility input.

**Finding and rationale:** The repository documents backup, restore, and rollback expectations but cannot enforce or prove that the homelab performed them. A deployment can otherwise proceed without a verified backup or with a volume/project mismatch. A text checklist is not an operational result.

**Action:** Require a homelab pre-deploy check of Stack/project and volume identity plus a valid database-and-images backup manifest. For a schema-changing release, require the OPS-01 compatibility/rollback decision. After deployment, check service health, a database-backed request, image write/read, and continuity of existing data. Keep any release gate, snapshot, and restore operation in the homelab-owned deployment process.

**Evidence:** [Deployment backup and post-deploy checklist](../self-hosted-deployment.md#L44-L54) and [homelab ownership](../self-hosted-deployment.md#L3-L18).

**Verification and trade-offs:** Exercise a controlled failed rollout/rollback against an isolated restored environment and demonstrate return to a usable compatible application without data loss. Record the gate inputs and result. Manual approval is valuable around irreversible migrations but can slow small releases; apply it where data/schema risk warrants it rather than adding a gate that merely repeats prose without checking real artifacts.

### OPS-08 — Tune resource limits only from host measurements

**Priority:** P3 / optional. **Trigger:** observed OOM kills, sustained resource pressure, or latency at the current limits under representative household use.

**Action:** If triggered, collect container memory/CPU/PID and PostgreSQL usage over representative periods, including circuit/user concurrency. Establish a baseline and alert on OOM events before tuning Compose limits. Change one limit at a time based on measured headroom and the target host's capacity; do not raise every limit preemptively.

**Evidence:** [Compose resource budgets for PostgreSQL](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L22-L27), [API](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L54-L59), and [Web](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L101-L106).

**Verification and trade-offs:** Compare working-set/peak memory and OOM counts before and after a representative load, with application latency and database health. No capacity fault is established by the static limits alone. More memory can reduce headroom for the host; lower limits can trigger avoidable restarts, so tune from measurements, not assumptions.

## Observability findings

### OBS-01 — Add workflow spans/metrics only if HTTP traces do not answer operational questions

**Priority:** P3 / optional. **Trigger:** after OTLP delivery is proven, operators still cannot distinguish importer stages or shopping-list regeneration time from the surrounding HTTP request.

**Action:** If triggered, add a small, bounded set of spans around import fetch/extract/validate/persist and shopping-list regeneration, plus low-cardinality duration/outcome metrics where they help an operator choose an action. Do not record raw recipe titles, full URLs, HTML, or other user/source content as tags or metric labels. Do not duplicate HTTP instrumentation already provided by service defaults.

**Evidence:** [OpenTelemetry setup includes HTTP, HttpClient, and runtime instrumentation plus conditional OTLP export](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.ServiceDefaults/Extensions.cs#L48-L96); the reviewed [import service workflow](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportService.cs#L41-L79) and [shopping service](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Shopping/Lists/ShoppingListService.cs#L103-L185) do not show domain spans in the reviewed sections.

**Verification and trade-offs:** With the intended collector connected, capture successful, failed, and cancelled workflow traces and confirm stages, durations, and outcome are useful without high-cardinality or sensitive fields. Add instrumentation only if an operator can name the decision it supports; extra spans increase telemetry volume and maintenance and should not precede the storage/recovery gates.

### OBS-02 — Prove DB and image-storage readiness separately from liveness

**Priority:** P1 for the operational checks before rollout; adding a recurring storage readiness probe is conditional on the chosen readiness contract. **Owner:** homelab for host mount; application for health semantics.

**Finding and rationale:** `/health` selects checks tagged `ready`, currently including the database connectivity check; `/alive` selects the self/liveness check. The image storage path is not checked for writeability. Compose uses `/health` for API health, so it can be healthy while the host image bind mount is unwritable. Conversely, a storage probe should not make process liveness depend on the image filesystem.

**Action:** Before rollout, verify the host ownership/mode and perform the OPS-04 write/read smoke test plus a database-backed API operation. If continuous readiness must reflect image-storage failure, add a bounded, safe storage check to readiness (or a separate diagnostic endpoint) while keeping liveness independent. A probe should create, read, and delete only its own unique test object; it must not modify user photos or write on every liveness request.

**Evidence:** [API registers the database as the `ready` health check and maps `/health`](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Program.cs#L20-L27), [health and liveness endpoint predicates](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.ServiceDefaults/Extensions.cs#L108-L130), and [DB connectivity check](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Infrastructure/Data/AppDbHealthCheck.cs#L5-L22). The current [integration upload check](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/tests/WhatToCook.Api.IntegrationTests/Specs/SelfHostedWebPlatformSpecificationTests.cs#L63-L92) asserts a file exists under its test storage root; it does not prove production host permissions or GET/readback through the deployed photo route.

**Verification and trade-offs:** Use the real deployment mount and the application's `1654:1654` identity for the safe, disposable-recipe smoke described in OPS-04; perform a DB-backed request as well. For a controlled negative test, use an isolated deployment with a separate read-only test mount, not the live image mount, and confirm readiness becomes unhealthy if storage is included while `/alive` remains healthy. Periodic filesystem probes add I/O and can cause unnecessary recovery actions if transient; prefer pre-deploy and post-deploy smoke as the baseline unless continuous storage readiness is an explicit requirement.

### OBS-03 — Confirm application SLI/SLO, alert ownership, and runbooks in the homelab

**Priority:** P2 when operators rely on alerts for service recovery. **Owner:** homelab for alert delivery and runbooks; application owner for any missing signal.

**Finding and rationale:** The application repository documents health endpoints and OTLP configuration but cannot show whether the homelab collector, dashboards, alerts, notification routes, or runbooks exist. Do not infer that monitoring is absent outside this repository, or that configured OTLP is a working alerting system.

**Action:** Ask the homelab owner to confirm delivery and ownership, then fill only demonstrated gaps. A useful initial SLI set is API 5xx rate and latency, import failure ratio by supported adapter, photo upload failure, database readiness, and image-storage capacity/free space. Set SLO thresholds from service expectations and observed baseline rather than copying arbitrary targets. Give each actionable alert an owner and runbook; the host/volume alerts remain homelab-owned.

**Evidence:** [Health/telemetry contract](../self-hosted-deployment.md#L44-L68); [API and runtime instrumentation setup](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.ServiceDefaults/Extensions.cs#L48-L96).

**Verification and trade-offs:** Trigger a safe test alert and verify notification delivery, named ownership, and a usable runbook; test one database or storage signal as well as process availability. Do not call an alert delivered because an exporter endpoint is configured. Defining SLOs without service expectations can create noise; begin with a small set and adjust from measured behavior.

### OBS-04 — Preserve actionable importer failure categories without logging source content

**Priority:** P2. **Owner:** application.

**Finding and rationale:** The reviewed importer catches all exceptions from fetch and returns a shared unsupported-source result without preserving a diagnostic category in that path. This can make timeout, host validation, HTTP, and parsing failures hard to distinguish. It does not justify logging the full requested URL or HTML, which may contain private data or tokens.

**Action:** Classify expected fetch/extraction failures (for example timeout/cancellation, disallowed host, HTTP status, oversized/non-HTML response, and unsupported extraction) into bounded internal categories and log the category, adapter, and duration. Preserve cancellation instead of converting caller cancellation into an ordinary failure. Keep user-facing error responses safe and stable; do not log raw HTML or full URLs/query strings.

**Evidence:** [Broad fetch catch and generic result](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportService.cs#L62-L79); [fetch constraints](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Imports/RecipeImportPageFetcher.cs#L17-L75).

**Verification and trade-offs:** Exercise timeout, blocked-host, non-success HTTP, invalid/oversized HTML, extraction failure, and cancellation cases. Confirm the API returns controlled user-facing results, logs a useful bounded category, and leaves cancellation observable without any raw URL/query or page body. More specific categories help triage but can expose source data if log fields are too detailed; keep categories low-cardinality and sanitized.

### OBS-05 — Define log/trace redaction and cardinality policy, then check the collector

**Priority:** P2 to confirm the policy; implementing changes can remain P3 if the verified policy already prevents disclosure. **Owner:** application and homelab/collector owner.

**Finding and rationale:** The application logs formatted messages/scopes to OpenTelemetry, and a recipe write route logs user-supplied titles. The repository review did not establish collector-side redaction or retention policy. This creates a privacy and telemetry-cardinality question, not evidence that a secret or personal data has been disclosed. Full importer URLs and exception data must not become telemetry fields by accident.

**Action:** Agree allowed fields and retention for application logs, spans, and metrics. Prefer stable IDs and bounded categories over titles or full URLs; redact query strings and known secret patterns before export; cap metric label cardinality; and review exception/log scopes. Verify collector-side processing rather than assuming application settings provide the complete policy. Consider sampling only after preserving enough data for incident diagnosis.

**Evidence:** [OpenTelemetry logging includes formatted messages and scopes](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.ServiceDefaults/Extensions.cs#L48-L55); [recipe title is included in application log messages](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/Features/Recipes/Editor/EditorEndpoints.cs#L36-L60); [production console log configuration](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.Api/appsettings.Production.json#L8-L20).

**Verification and trade-offs:** Import a controlled URL containing a harmless test query token and inspect app logs, exported traces, collector output, and metric labels. Confirm the token, raw URL, page content, and unnecessary recipe title do not appear where policy forbids them; verify normal diagnostic fields remain. Redaction can remove useful context, while unbounded labels increase storage cost; make that trade-off explicit and retain only fields needed for a stated operational purpose.

### OBS-06 — Verify end-to-end OTLP delivery if homelab telemetry is part of operations

**Priority:** P2 to confirm when the service is expected to be monitored; P3 if the deployment intentionally does not use an OTLP backend. **Owner:** homelab for collector/backend; application for exporter configuration.

**Finding and rationale:** The service defaults use OTLP when an endpoint is configured, and Compose provides a collector endpoint default. That proves neither DNS/TLS/protocol reachability nor that traces/metrics arrive in a backend. No live trace, metric, or alert was observed in this review.

**Action:** Have the homelab owner confirm the collector/backend and the actual endpoint/transport settings. If telemetry is required, run a controlled app request and verify its trace and at least one relevant metric through the full export path; correct only demonstrated gaps. Keep credentials and collector secrets in the existing homelab-owned injection path, not in the application repository.

**Evidence:** [Compose OTLP endpoint default](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/deploy/docker-compose.yml#L63-L72); [conditional OTLP exporter registration](https://github.com/Kyziur/WhatToCook/blob/4eb64d7e3de01faa7bef32db74da94e7e1c50913/src/WhatToCook.ServiceDefaults/Extensions.cs#L86-L96).

**Verification and trade-offs:** Capture a controlled trace ID from an application request and locate it in the backend; verify a metric and collector health separately. Repeat after an endpoint/TLS/network change. If telemetry is deliberately disabled, record that operational choice and rely on the agreed local health/log path. End-to-end telemetry improves diagnosis but consumes network, storage, and operator time; do not equate an environment variable with successful delivery.

## Verification handoff and caveats

Before production use, the homelab owner should retain evidence for the trusted/untrusted network matrix, current project and volume identity, backup manifest and isolated restore, host ownership/mode, and post-deploy DB-backed plus photo write/read checks. The application owner should retain targeted regression/negative-test evidence for any changed authentication, CSRF, importer egress, metadata handling, health semantics, or telemetry behavior. Keep scan output, restore evidence, and live network/alert results distinct from repository configuration.

No build, test, lint, formatter, live network probe, security scan, backup, or restore drill was run while preparing this documentation. A documented procedure is not proof that it ran; configured network membership is not proof of live reachability; a health endpoint is not proof of image storage read/write; and the absence of findings in this static review is not proof of absence of a vulnerability or incident.