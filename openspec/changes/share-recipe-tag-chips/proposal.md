# Shared recipe tag controls

## Why
Recipe editor and imported-draft review duplicate the same list of removable tag chips and drift in the close symbol and accessible labels.

## Changes
Use one feature-local presentational component for tag display and removal. Each owning page keeps its independent tag data, validation, suggestions and submission logic. Add an accessible name for each remove action and bUnit tests for dispatch.

## Boundaries
No generic form library, shared mutable state, API contract or persistence change. Preserve the existing chip styles and remove behavior. Scoped to the two recipe editing workflows.
