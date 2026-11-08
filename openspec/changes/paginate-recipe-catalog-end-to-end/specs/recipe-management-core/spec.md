## ADDED Requirements

### Requirement: Library catalog search and filtering cover all active recipes with bounded pages
The system SHALL expose stable, bounded paged results, applying selected filters and sorting to the entire active catalog before selecting a page.

#### Scenario: Find an active recipe beyond the first 500
- **WHEN** more than 500 active recipes match a library or search query
- **THEN** a client can navigate to every matching result using pages of at most 50, with total count consistent with the filters

#### Scenario: Preserve library and legacy search tag semantics
- **WHEN** multiple tags are selected in the library
- **THEN** the library matches recipes having all selected tags, while the legacy search API continues to match any tag

#### Scenario: Filter facets are not limited to the current page
- **WHEN** a tag or ingredient occurs only on later catalog pages
- **THEN** it remains available as a library facet while the first page is displayed

#### Scenario: Stable browsing with equal sort values
- **WHEN** recipes have equal titles or servings
- **THEN** each page has a deterministic ID tie-breaker and page navigation does not duplicate rows in an unchanged catalog
