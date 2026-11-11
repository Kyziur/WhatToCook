# SEC-04 — Browser mutation guard for internal API

## Why
The application uses server-side Blazor Web calling an internal API over HTTP. Browsers are not intended to call API mutation routes directly, yet cross-origin simple form uploads may reach them from a browser on the same trusted network. The existing photo route disables antiforgery and authentication is not currently part of the internal API trust boundary.

## Change
Reject browser-originated unsafe HTTP methods under `/api` when `Origin` is present or `Sec-Fetch-Site` indicates a browser context other than `none`. Server-to-server requests with no browser metadata remain supported. This is a deliberately scoped defense-in-depth guard, not a substitute for API authorization, network isolation or a future cookie-authenticated CSRF solution.

## Limits
Browser-origin headers can be absent or spoofed by non-browser clients. Do not treat this as a strong authorization boundary. Reassess all mutation routes before accepting browser credentials or widening network exposure.
