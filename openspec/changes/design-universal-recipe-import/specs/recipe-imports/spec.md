## ADDED Requirements

### Requirement: URI-01 — Recipe importer can extract public Schema.org Recipe metadata
The system SHALL support importing publicly reachable HTTP(S) recipe pages containing usable Schema.org `Recipe` structured data into untrusted import drafts, while retaining existing source-specific adapters.

#### Scenario: URI-01 — Extract a structured Recipe document
- **WHEN** a user submits a public supported HTML page containing a valid `Recipe` JSON-LD object
- **THEN** the system creates a persisted draft with extractable title, source URL, servings when available, ingredients, instructions, and other supported fields without creating a trusted recipe

#### Scenario: URI-01 — Recognize common JSON-LD layouts
- **WHEN** the page has a `Recipe` object at the JSON-LD root, inside an array, or nested in an `@graph`, with `@type` as string or string array
- **THEN** the importer finds relevant recipe objects without mistaking unrelated structured entities for recipes

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

#### Scenario: URI-02 — Resolve multiple recipes on one page
- **WHEN** a page exposes multiple plausible `Recipe` objects
- **THEN** the user can select the intended recipe or is prompted to disambiguate before an incorrect draft is finalized

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

#### Scenario: URI-05 — Recognize already imported source URLs
- **WHEN** a submitted URL matches the normalized source of an existing recipe
- **THEN** the importer offers to open the existing recipe rather than automatically creating a second one

#### Scenario: URI-05 — Detect title conflicts in review
- **WHEN** the extracted normalized title matches a saved recipe
- **THEN** the reviewer can inspect the existing entry or correct the draft; normal title uniqueness remains enforced at finalization

#### Scenario: URI-05 — Do not merge unrelated recipes
- **WHEN** recipes originate from the same domain or from URLs with semantically meaningful different paths or query components
- **THEN** they are not automatically merged or treated as identical solely because of their source site

### Requirement: URI-06 — Import errors are recoverable and honest
The system SHALL explain why a URL import did not succeed and provide a safe alternative to manual entry.

#### Scenario: URI-06 — Explain invalid, inaccessible and unsupported links
- **WHEN** URL submission fails due to invalid input, network/website restrictions, or unavailable structured recipe data
- **THEN** the UI displays a distinguishable reason and offers retry with another URL or manual entry

#### Scenario: URI-06 — Preserve trustworthy draft boundaries
- **WHEN** some extracted content is shown despite incomplete data
- **THEN** it is identified as an untrusted draft requiring review and not as a successfully imported trusted recipe

#### Scenario: URI-06 — Retry transient failures without duplication
- **WHEN** a user retries after a timeout or temporary fetch failure
- **THEN** the import can be attempted again without silently creating duplicate trusted recipes

#### Scenario: URI-06 — Do not equate HTTP success with recipe success
- **WHEN** the remote server responds with HTTP 200 but no valid extractable recipe information
- **THEN** the application reports unsuccessful extraction instead of displaying a false success confirmation
