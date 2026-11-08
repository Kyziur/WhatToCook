## ADDED Requirements

### Requirement: Planner recipe selection can access every active recipe

The planner SHALL load active recipe summaries beyond the legacy 500-item API cap using bounded stable server pages, without changing existing planner selection semantics.

#### Scenario: Planner has more than 500 recipes
- **WHEN** an active catalog contains 551 recipes and the planner loads available recipes
- **THEN** all 551 recipe summaries remain available, including the final page, and no request uses the legacy capped catalog endpoint

#### Scenario: Planner avoids repeated catalog facet work
- **WHEN** the planner traverses the paged API with `includeFacets=false`
- **THEN** each page preserves its recipe entries and total count, but empty tag/ingredient facets are returned without computing full-catalog facets

#### Scenario: Existing library clients retain facets
- **WHEN** a client omits `includeFacets` on the same page API
- **THEN** the response retains the existing full-catalog tag/ingredient facets and counts

#### Scenario: Short nonempty page is not mistaken for complete catalog
- **WHEN** the server returns fewer items than the declared total across otherwise nonempty pages
- **THEN** the planner reports a load error instead of silently dropping the remaining recipes

#### Scenario: Page traversal fails before the declared total
- **WHEN** a later page is unexpectedly empty before the declared `TotalCount`
- **THEN** the planner client reports a load error instead of silently hiding recipes
