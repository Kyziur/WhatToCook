# ARC-01 targeted — evidence

## Source review
The original new-recipe path persisted a recipe, then attempted the photo upload. A failed upload left the component in create mode; a subsequent form save passed `RecipeId == null` to the service. The change remembers the first successful create response ID before the upload, reuses it on retry, and leaves edit mode behavior unchanged.

## Verification status
- New bUnit theory covers returned failure and exception, followed by successful retry.
- Existing no-photo create test remains the success control.
- **CI and independent review not yet complete.** This is review evidence, not a claim of passing tests or production readiness.
- Circuit loss, lost create response, and server-side request idempotency are intentionally outside scope.
