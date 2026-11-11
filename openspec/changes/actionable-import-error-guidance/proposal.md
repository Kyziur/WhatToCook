# UX-05 — Actionable recipe import failures

## Why
The importer already records bounded internal failure categories but returns a generic failure message to Web. Users cannot tell a timeout from an unsupported source or a blocked destination.

## Scope
Extend create-import error responses with a stable `code` field while preserving status codes and the existing `message`. Map only those bounded codes to Polish guidance in the Web client. No raw URL, HTML or exception details in responses. Preserve request cancellation. Keep other import-draft routes and response contracts unchanged.

## Risk/compatibility
The extra JSON property is additive. Web falls back to a safe generic message on unknown future codes. Diagnostic codes do not authorize destination access or weaken SSRF checks.
