## ADDED Requirements

### Requirement: URI-01 — Recipe importer can extract public Schema.org Recipe metadata
The system SHALL extract usable Schema.org `Recipe` data from publicly reachable safe HTTP(S) pages, retaining specialized adapters. A **single unambiguous candidate** may create a persisted draft immediately; a page with multiple candidates creates a **bounded temporary selection attempt**, not an arbitrarily selected draft.

#### Scenario: URI-01 — Extract an unambiguous structured recipe
- **WHEN** a user submits a safe public HTML page containing exactly one usable `Recipe` JSON-LD candidate
- **THEN** the system creates a persisted draft with available title, source URL, servings, ingredients and instructions, without creating a trusted recipe

#### Scenario: URI-01 — Recognize common JSON-LD layouts
- **WHEN** the page has a `Recipe` object at the JSON-LD root, inside an array, or nested in an `@graph`, with `@type` as a string or string array containing either the compact name `Recipe` or a Schema.org IRI such as `https://schema.org/Recipe`
- **THEN** the importer recognizes the recipe type in both representations without mistaking unrelated structured entities for recipes

#### Scenario: URI-01 — Preserve meaningful instruction order and text
- **WHEN** the recipe contains instruction text, `HowToStep`, or ordered `HowToSection` structures
- **THEN** the draft displays instructions in a sensible sequential order, preserving Unicode and Polish characters while omitting navigation, scripts, and layout content

#### Scenario: URI-01 — Handle partial recipe fields
- **WHEN** structured data omits quantities, servings, or other required fields or contains ambiguous `recipeYield`
- **THEN** the importer preserves available data, flags missing or ambiguous fields for review, and never fabricates ingredient values

#### Scenario: URI-01 — Report unextractable sources
- **WHEN** a page has no usable Recipe data and no supported source-specific adapter
- **THEN** the user sees an actionable unsupported-source outcome and can continue with manual entry without creating a trusted recipe

### Requirement: URI-02 — User can verify and correct an imported draft
The system SHALL use the existing editable import-draft lifecycle and finalization rules for generically extracted recipes.

#### Scenario: URI-02 — Inspect and correct a draft
- **WHEN** a user opens a draft from a structured recipe page
- **THEN** the UI displays editable title, servings, source URL, ingredients, steps, and tags, with missing mandatory fields clearly identified

#### Scenario: URI-02 — Persist draft corrections
- **WHEN** a user changes draft values or reorders, removes, or merges preparation steps
- **THEN** the corrected draft is persisted and reopened with the user's changes intact, without writing into trusted recipe tables

#### Scenario: URI-02 — Persist a temporary multi-recipe selection attempt
- **WHEN** a fetched page exposes multiple plausible `Recipe` objects
- **THEN** the importer persistently stores a bounded, untrusted **extracted field snapshot for each candidate** (including data needed to populate the later draft) plus its stable candidate ID and safe summary, and returns an explicit selection-required response without creating a trusted recipe or silently choosing a draft

#### Scenario: URI-02 — Select from the original extracted snapshot
- **WHEN** the user selects a candidate in a valid pending attempt after the source webpage has changed or become unavailable
- **THEN** the draft is created from the bounded candidate snapshot captured for that attempt, without re-fetching arbitrary external content or accidentally selecting a different recipe

#### Scenario: URI-02 — Select one candidate and create a single draft
- **WHEN** a user selects a candidate from a valid pending import attempt
- **THEN** exactly that candidate becomes the persisted editable import draft and repeated selection of the same candidate returns the same draft instead of creating duplicates

#### Scenario: URI-02 — Reject a second candidate selection from an already selected attempt
- **WHEN** the user tries selecting a different candidate from an attempt that has already created a draft
- **THEN** the server reports the existing selected draft instead of creating another draft or overwriting the first selection

#### Scenario: URI-02 — Expire or cancel multi-recipe attempts
- **WHEN** the user cancels selection or the attempt expires after 24 hours
- **THEN** no trusted recipe is created by that cancellation/expiry, no selection is possible after the attempt is cancelled or expired, and stored candidate snapshots are deleted or made inaccessible according to the retention policy; an already selected and finalized draft remains governed by the existing draft lifecycle

#### Scenario: URI-02 — Finalize with existing validation
- **WHEN** a user finalizes a valid reviewed draft
- **THEN** the shared recipe write rules create a trusted recipe once; missing mandatory values and normalized duplicate titles block finalization without deleting the draft

#### Scenario: URI-02 — Retry finalization safely
- **WHEN** the user repeats finalization because of a timeout or an already finalized draft
- **THEN** the backend does not create duplicate trusted recipes and returns a consistent outcome

### Requirement: URI-03 — Arbitrary URL imports are protected from hostile sources
The system SHALL enforce safe outbound fetching, bounded parsing, and untrusted-content handling before broadening recipe URL support.

#### Scenario: URI-03 — Reject unsafe URL schemes and network destinations
- **WHEN** a submitted URL uses an unsupported scheme or resolves or connects to loopback, private, link-local, metadata-service, or disallowed internal service addresses, including ambiguous IPv4/IPv6 encodings
- **THEN** the server rejects the fetch without exposing internal resources

#### Scenario: URI-03 — Protect against rebinding and redirects
- **WHEN** a source address changes between resolution and connection or an HTTP redirect targets a forbidden address
- **THEN** the request is refused or the redirected connection revalidates the actual target; disabling redirects is acceptable if documented

#### Scenario: URI-03 — Bound untrusted remote input
- **WHEN** a page responds slowly, uses an unsupported content type, exceeds the allowed size, or contains deeply nested or malformed JSON-LD
- **THEN** the importer fails within documented time, memory, and parsing limits without exhausting the service

#### Scenario: URI-03 — Avoid active-content execution
- **WHEN** a remote page or extracted field contains HTML scripts, event handlers, or untrusted markup
- **THEN** no remote script is executed and the review UI displays content safely without injecting executable markup

#### Scenario: URI-03 — Avoid leaking internal details in errors
- **WHEN** fetching or parsing fails
- **THEN** the user receives a safe, actionable error without infrastructure hostnames, network details, or secrets

### Requirement: URI-04 — User can initiate recipe import from a mobile share action
The system SHALL offer a phone-friendly path from a shared recipe URL into the existing import draft workflow, with a manual URL fallback.

#### Scenario: URI-04 — Accept an OS/browser share target
- **WHEN** a compatible phone browser shares a recipe URL to the installed WhatToCook experience
- **THEN** the import flow opens with the intended URL prefilled and visibly identified, awaiting explicit user confirmation to import

#### Scenario: URI-04 — Handle shared text containing a URL
- **WHEN** the shared payload contains commentary and one or more possible URLs
- **THEN** the user can identify the desired URL rather than silently importing the wrong content

#### Scenario: URI-04 — Fall back when sharing is unavailable
- **WHEN** the current operating system, browser, origin, or PWA installation does not support share-target registration
- **THEN** a clearly available manual paste/import flow remains usable

#### Scenario: URI-04 — Handle repeated or invalid shares
- **WHEN** a user shares an invalid or previously used link
- **THEN** the app reports the problem or shows existing draft/recipe choices and does not silently overwrite saved data

### Requirement: URI-05 — Import avoids accidental duplicates
The system SHALL warn about matching sources and normalized recipe titles while leaving the user in control of draft review.

#### Scenario: URI-05 — Normalize source URLs without erasing recipe identity
- **WHEN** URLs differ only by host casing, default port, or known tracking query keys (e.g. `utm_*`, `fbclid`, `gclid`)
- **THEN** they may be treated as the same source; scheme, meaningful path and query differences, and URL fragments are preserved by default because fragments may encode distinct client-side recipe routes. Removing an explicitly known non-identifying presentation fragment requires a source-specific rule

#### Scenario: URI-05 — Offer a soft source-match warning
- **WHEN** the normalized submitted source URL matches an existing trusted recipe
- **THEN** the importer offers to open it while still permitting review of a new draft; source matches alone never silently block importing

#### Scenario: URI-05 — Detect title conflicts in review
- **WHEN** the extracted normalized title matches a saved recipe
- **THEN** the reviewer can inspect the existing entry or correct the draft; normal title uniqueness remains enforced at finalization

#### Scenario: URI-05 — Preserve distinct sources
- **WHEN** two recipe URLs differ in meaningful path segments or preserved query parameters, or only share a domain
- **THEN** they are not automatically merged or blocked as duplicates solely due to domain/source similarity

### Requirement: URI-06 — Import errors are recoverable and honest
The system SHALL explain why a URL import did not succeed and provide a safe alternative to manual entry.

#### Scenario: URI-06 — Explain invalid, inaccessible and unsupported links
- **WHEN** URL submission fails due to invalid input, network/website restrictions, or unavailable structured recipe data
- **THEN** the UI displays a distinguishable reason and offers retry with another URL or manual entry

#### Scenario: URI-06 — Preserve trustworthy draft boundaries
- **WHEN** some extracted content is shown despite incomplete data
- **THEN** it is identified as an untrusted draft requiring review and not as a successfully imported trusted recipe

#### Scenario: URI-06 — Retry transient failures without duplication
- **WHEN** a user retries after a timeout or temporary fetch failure, or retries selection/finalization with the same operation identity
- **THEN** the importer does not silently create duplicate trusted recipes or duplicate selected drafts

#### Scenario: URI-06 — Do not equate HTTP success with recipe success
- **WHEN** the remote server responds with HTTP 200 but no valid extractable recipe information
- **THEN** the application reports unsuccessful extraction instead of displaying a false success confirmation
