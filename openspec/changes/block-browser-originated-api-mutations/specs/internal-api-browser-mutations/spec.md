# Browser mutation boundary

## ADDED Requirements

### Requirement: Internal API rejects browser-initiated mutations
The internal API SHALL reject unsafe `/api` requests with an `Origin` header or cross-site browser Fetch Metadata with HTTP 403, without running mutation handlers.

#### Scenario: Cross-origin recipe creation
- **WHEN** a browser-originated POST supplies an Origin header
- **THEN** the response is 403 and no mutation handler executes.

#### Scenario: Cross-site multipart photo upload
- **WHEN** a multipart upload includes `Sec-Fetch-Site: cross-site`
- **THEN** it returns 403 without writing a recipe photo.

### Requirement: Existing internal server calls remain supported
Requests from the server-side Web application that do not send browser metadata SHALL retain the existing API behavior. Read-only endpoints and health checks SHALL not be gated by the browser mutation guard.

### Requirement: Enforcement is not claimed as authorization
The application SHALL continue requiring network isolation and exposure review; this filter SHALL NOT replace identity or authenticated CSRF enforcement when browser access changes.
