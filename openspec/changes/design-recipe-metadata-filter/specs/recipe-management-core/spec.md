## MODIFIED Requirements

### Requirement: User can browse and find active recipes quickly
The system SHALL provide a recipe library that supports alphabetical browsing, searching, and filtering of active recipes by title, tags, normalized ingredients, and the supported preparation-time metadata bounds so the user can quickly locate a recipe to cook, edit, or plan.

#### Scenario: Search by title
- **WHEN** the user enters a search phrase matching a recipe title after case-insensitive and diacritic-insensitive normalization
- **THEN** the system narrows the visible recipe list to matching recipes

#### Scenario: Search by tag
- **WHEN** the user searches by one or more tags
- **THEN** the system narrows the visible recipe list to recipes matching those tags

#### Scenario: Search by all selected ingredients
- **WHEN** the user selects multiple normalized ingredients while searching
- **THEN** the system returns recipes containing at least all selected ingredients, even if the recipes contain additional ingredients

#### Scenario: Browse recipes alphabetically
- **WHEN** the user opens the recipe library without active search or filters
- **THEN** the system shows active recipes in alphabetical order by title

#### Scenario: Filter by preparation-time range
- **WHEN** the user supplies an optional inclusive minimum and/or maximum preparation time in minutes
- **THEN** the system returns only active recipes with a recorded preparation time inside the requested bounds

#### Scenario: Keep recipes with missing preparation time out of an active range
- **WHEN** a recipe has no recorded preparation time and a preparation-time filter is active
- **THEN** the system excludes that recipe from the filtered results without treating the missing value as zero

#### Scenario: Preserve metadata filter state in the library URL
- **WHEN** the user applies a valid preparation-time filter and navigates away or refreshes the library
- **THEN** the filter bounds remain represented in the library URL and the same filtered result is restored

#### Scenario: Show result summary and clear an active metadata filter
- **WHEN** the user has an active preparation-time filter in the recipe library
- **THEN** the system shows the filtered result count and a clear action; clearing the filter removes its bounds and restores the unfiltered result set

#### Scenario: Reject an invalid metadata range
- **WHEN** the user submits a zero or negative bound or a minimum greater than the maximum
- **THEN** the system reports a validation error and does not silently apply a different filter
