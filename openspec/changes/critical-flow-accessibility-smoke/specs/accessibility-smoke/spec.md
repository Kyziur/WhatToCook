# Accessibility smoke

## ADDED Requirements

### Requirement: Critical entry routes are scanned for accessibility defects
The Playwright CI suite SHALL scan the recipe library, import URL, and shopping routes for automatically detectable WCAG 2.2 A/AA violations in both configured viewport projects, and fail when there are violations. It SHALL first wait for route-specific Blazor interaction readiness, then wait for route data and asynchronous content to settle before scanning.

#### Scenario: Critical route loads
- **WHEN** the page's main heading is visible and route-specific controls or fixture interaction confirm Blazor interactivity
- **THEN** no route content remains busy and an axe scan reports no detected violations for WCAG 2 A/AA, 2.1 A/AA, 2.2 AA.

#### Scenario: Shopping scan uses its own generated fixture
- **WHEN** the shopping scan creates a unique fixture recipe and plan and opens `/shopping` with that plan's explicit ID
- **THEN** the selected plan and at least one generated shopping item are visible before the axe scan
- **AND** the test toggles a fixture checklist item and restores its state to prove Blazor event handling is active
- **AND** the scenario does not depend on plans or list data created by another test
- **AND** the fixture data is discarded with the per-run Playwright database.

### Requirement: Keyboard operation of core inputs
The suite SHALL verify keyboard focus and text entry for the recipe search and import URL by tabbing from the actual navigation focus point (`h1`, focused by `FocusOnNavigate`), not by programmatically focusing either target; URL submission SHALL be reachable by Tab.

#### Scenario: Search is keyboard reachable
- **WHEN** the user starts from the route heading focused on navigation and presses Tab through the page
- **THEN** focus reaches the visible, labelled recipe search field and keyboard text entry updates its value.

#### Scenario: Import URL without pointer
- **WHEN** the user tabs from the navigation-focused route heading to the URL field, types a value, and presses Tab
- **THEN** the primary submit control receives focus and is enabled.
