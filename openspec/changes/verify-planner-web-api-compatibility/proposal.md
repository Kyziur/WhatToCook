# ARC-05 — Verify Blazor planner API compatibility

## Why
The API planning endpoints own feature-local response/request contracts, while the Blazor Web `ApiMealPlanService` maintains private DTOs and maps them independently. Existing API endpoint tests and Web tests separately validate behavior, but no focused test guarantees the **real Web client** can deserialize and submit requests against the **real API response**.

## What changes
- Exercise `ApiMealPlanService` from the API integration-test project against the existing `TestWebApplicationFactory` and PostgreSQL Testcontainers stack.
- Cover create, list, details, regeneration-required conflict, regenerate/save and read-back through the actual client mapper.
- Reference the Web project only from the test project; do not move or centralize application DTOs.

## Non-goals
No API contract changes, migrations, OpenAPI snapshot, cross-feature contract assembly, production DI changes or deployment changes.

## Risk / verification
Cross-project test compilation and serializing `DateOnly`, nullable response fields and conflict status mapping must pass full PR CI. An integration test is a verification fixture, not a new production dependency.
