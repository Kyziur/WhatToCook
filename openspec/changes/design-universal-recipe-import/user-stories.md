> Supplementary user stories and detailed acceptance criteria for a **design-only** OpenSpec change. The future capability delta is [specs/recipe-imports/spec.md](specs/recipe-imports/spec.md). This proposal depends on the still-open [add-recipe-import](../add-recipe-import/proposal.md) change and must not be synced until the base import capability is established.

# Epic URI — Universal Recipe Import

**Priority:** P1 — third proposed product epic  
**Status:** Proposed / not implemented  
**Users:** Home cook discovering recipes on the web, household recipe curator  
**Primary outcome:** Bring a recipe from a supported structured website into WhatToCook with minimal manual retyping.

## Problem and job to be done

WhatToCook currently offers a URL-based import draft, review and correction, finalization, and a small set of site-specific adapters. Users discover recipes on many other sites, often on phones, and copying ingredients/steps into the editor is tedious. Structured recipe information is commonly provided via Schema.org `Recipe` data (often JSON-LD), although sites may omit fields or use inconsistent formats.

**Job story:** *When I find a recipe worth keeping on a website, I want to send its URL to WhatToCook, review the extracted details, and save it so that I can use it later without copying the whole recipe by hand.*

## Existing functionality to retain

- A source URL is submitted and inspected **only** by the server's import service.
- The result is a persisted, untrusted import draft, not an automatic trusted recipe.
- The user reviews/corrects title, servings, ingredients, steps, source, and tags before finalization.
- Normal recipe validation and unique normalized title rules apply on finalization.
- Existing source-specific adapters remain available for sites with useful dedicated parsing.

## MVP user stories and acceptance criteria

### URI-01 — Import recipes using structured website data

**As a** cook, **I want** to import recipes from websites that publish structured recipe metadata **so that** I am not limited to a handful of hard-coded sites.

**Acceptance criteria**

- **AC1:** Given a safe public HTTP(S) page with **one unambiguous** Schema.org `Recipe` candidate, submitting its URL creates a draft. Multiple candidates require explicit selection before draft creation.
- **AC2:** The import can find a `Recipe` object nested in a `@graph`, an array of JSON-LD objects, or a single root object, without assuming one fixed placement.
- **AC3:** A `@type` value represented as a string or array can be recognized; unrelated structured objects are not imported as recipes.
- **AC4:** Extracted title, recipe yield/servings when unambiguous, ingredients, preparation instructions, source URL, and available supported metadata are mapped to the existing draft model.
- **AC5:** Instructions provided as plain text, `HowToStep`, or `HowToSection` are presented in a sensible readable order, without treating CSS/scripts/navigation as recipe steps.
- **AC6:** The system handles simple HTML entity decoding, Unicode, Polish diacritics, and common line breaks without damaging text.
- **AC7:** If the website has no usable structured recipe, the user receives a meaningful **Could not extract this recipe** outcome, and can still choose manual entry. The service does not invent missing ingredients.
- **AC8:** Import does not save to the trusted recipe catalog until the user completes the existing review/finalize flow.

**Implementation-neutral note:** Schema.org handling is the generic extraction path; a source-specific adapter may be preferred when it yields demonstrably better, validated data.

### URI-02 — Review and correct imperfect extraction

**As a** cook, **I want** to inspect imported data before committing it **so that** errors from the website do not pollute my catalog.

**Acceptance criteria**

- **AC1:** The review screen presents source URL, title, servings, ingredients, instructions, and tags in editable fields.
- **AC2:** Missing mandatory values are clearly marked and prevent finalization until corrected.
- **AC3:** Editing a draft and reopening it later preserves corrections.
- **AC4:** Duplicate normalized titles are detected and explained without discarding the draft.
- **AC5:** The user can remove accidental instructions, reorder instructions, and merge split steps using the existing draft-review behavior.
- **AC6:** Multiple plausible recipe objects create a **temporary untrusted import attempt** with candidate summaries. I choose one before a persisted editable draft is created; choosing the same candidate twice returns the same draft.
- **AC7:** Finalization is atomic and repeat-safe: retries must not create duplicate trusted recipes from the same finalized draft.
- **AC8:** An unselected import attempt expires after 24 hours; cancellation or expiry creates no trusted recipe and prevents later selection.

### URI-03 — Import safely from arbitrary recipe websites

**As a** household owner, **I want** importing external pages to be constrained **so that** a malicious link cannot access internal devices or overwhelm the server.

**Acceptance criteria**

- **AC1:** Only HTTP and HTTPS are supported; non-web schemes and invalid URLs are rejected before fetching.
- **AC2:** The fetcher does not access loopback, link-local, private, metadata-service, Docker/internal service or otherwise disallowed addresses, including IPv4-mapped IPv6 and other ambiguous address representations.
- **AC3:** The actual connection target is checked as well as DNS resolution, so DNS rebinding cannot bypass the internal-address restriction.
- **AC4:** Redirects, if followed, are bounded and must pass the same target checks at each hop. Disabling redirects entirely is also acceptable for MVP if communicated.
- **AC5:** Requests apply strict time, response-size, and content-type limits; malicious HTML and excessively nested JSON-LD do not cause unbounded processing or memory use.
- **AC6:** Retrieved scripts and active content are never executed as part of extraction; extracted HTML cannot inject executable markup into the review UI.
- **AC7:** Fetch failures, unsupported pages, and extraction errors are reported without exposing sensitive infrastructure addresses or credentials.
- **AC8:** Security tests cover local-address probes, external-to-private redirects, DNS rebinding simulation, oversized payloads, malformed JSON-LD, and unsupported schemes.

**Release dependency:** Security validation is a **must-pass gate** before expanding URL support; universal importing increases exposure to untrusted input.

### URI-04 — Add a recipe from a mobile share action

**As a** mobile user, **I want** to share a recipe URL from my browser to WhatToCook **so that** I do not need to switch apps and paste it manually.

**Acceptance criteria**

- **AC1:** From a compatible mobile browser/OS, the user can invoke a share action that passes a recipe URL to WhatToCook.
- **AC2:** WhatToCook opens the import flow with the URL prefilled and visibly identified; parsing does not begin silently if the user has not confirmed.
- **AC3:** If a share integration cannot be registered on the current device/browser, the app still supports a direct paste-from-clipboard workflow.
- **AC4:** The app handles shared text containing a URL plus surrounding commentary by extracting or asking the user to select the intended URL.
- **AC5:** Opening an unsupported or invalid shared URL shows a recoverable error, not a broken page.
- **AC6:** Repeated shares of the same URL do not silently overwrite an existing draft or recipe.
- **AC7:** The share integration must work with the deployed app's actual origin and security requirements; device-specific PWA/share-target limitations must be validated before claiming support.

**Delivery option:** URI-04 may follow URI-01/02 as a second vertical slice if OS/browser integration is not reliably available in the first release.

### URI-05 — Avoid duplicate work when importing a previously saved recipe

**As a** cook, **I want** to be told when a URL or recipe appears to be already in my library **so that** I do not accidentally save it twice.

**Acceptance criteria**

- **AC1:** When an equivalent normalized URL matches an existing recipe, I am **offered** a link to it but can still review a new draft. Source matching alone never blocks importing.
- **AC2:** When the imported normalized title conflicts with a catalog title, the review screen identifies the conflict and offers a path to inspect the existing recipe or change the new title.
- **AC3:** A source-URL match is treated as a helpful signal; distinct recipes with the same site or different pages are not automatically merged.
- **AC4:** I may explicitly continue working on a draft when its source overlaps an existing recipe, but standard unique-title validation still applies at finalization.
- **AC5:** URL normalization ignores host case, default ports, fragments and known tracking keys (`utm_*`, `fbclid`, `gclid`), but preserves the scheme and meaningful path/query differences, so two different recipes are not silently conflated.

### URI-06 — Recover gracefully when a website cannot be parsed

**As a** cook, **I want** to recover from failed extraction **so that** a blocked or unusual recipe site does not waste my effort.

**Acceptance criteria**

- **AC1:** Errors distinguish at least: invalid URL, unsupported webpage/recipe structure, unreachable or blocked site, and incomplete fields requiring review.
- **AC2:** A failure message offers **Try another link** and **Enter recipe manually**, and carries forward the URL when useful.
- **AC3:** Network timeouts and transient errors may be retried by the user but do not create multiple trusted recipes.
- **AC4:** Any partial extracted text displayed to the user remains clearly marked as untrusted draft data.
- **AC5:** The app never promises successful import simply because the remote page returned HTTP 200.

## Future opportunities (explicitly outside the first release)

- Photo / paper recipe OCR extraction using the existing draft review model.
- Automatic image download with content and license checks.
- Imports from PDFs, video descriptions, or social platforms.
- AI-based fallback extraction for non-structured websites with confidence warnings and cost controls.
- Batch importing URL collections.

## Release boundaries and data handling

- Use deterministic extraction first; do not make an LLM a required dependency.
- Preserve attribution and original recipe source link.
- Do not automatically fetch or persist every external image. A remote image URL is not equivalent to an approved image upload.
- Parsed JSON-LD is **untrusted**, even when the page is from a known food website.
- An extraction failure must not compromise existing manually entered recipes.
- Site terms and content ownership should be considered before supporting high-volume scraping; this epic addresses user-initiated single-recipe imports.

## Test plan and success metrics

**Automated cases**

- JSON-LD as root, `@graph`, array, and `@type` array.
- Ingredient strings with Polish diacritics, quantity/unit variants, and incomplete yield.
- Nested `HowToSection` plus `HowToStep` ordering.
- Multiple recipes in one page, malformed JSON-LD, no recipe data, and unreachable site.
- Corrections persisted on the draft, finalization idempotency, duplicate source/title.
- SSRF/DNS rebinding and payload-size tests using controlled fakes; no unsafe external endpoints needed.
- Mobile share behavior tested where supported and graceful paste fallback where not.

**Measure**

- Percentage of submitted recipe URLs resulting in a reviewable draft.
- Percentage of drafts successfully finalized.
- Median time from submit to final recipe (excluding deliberate user correction time, measured separately).
- Average number of field corrections per import source.
- Reported incorrect or misleading extracted recipe steps.

## Dependencies and implementation handoff

- Existing [recipe import change](../../../openspec/changes/add-recipe-import/proposal.md) and [recipe import spec](../../../openspec/changes/add-recipe-import/specs/recipe-imports/spec.md).
- Preserve existing adapter architecture, draft persistence, validation, and finalization.
- Plan additional DTO/model fields only for metadata actually displayed or used.
- Make network egress and SSRF review a separate mandatory engineering checkpoint before release.
- Agree OpenSpec deltas and scenario-level tests before implementation.
