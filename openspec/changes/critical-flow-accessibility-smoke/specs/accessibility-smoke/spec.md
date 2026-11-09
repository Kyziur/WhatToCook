# Accessibility smoke

## ADDED Requirements

### Requirement: Critical entry routes are scanned for accessibility defects
The Playwright CI suite SHALL scan the recipe library, import URL, and shopping routes for automatically detectable WCAG 2.2 A/AA violations in both configured viewport projects, and fail when there are violations.

#### Scenario: Critical route loads
- **WHEN** the page's main heading is visible and asynchronous content is no longer busy
- **THEN** an axe scan of the rendered page reports no detected violations for WCAG 2 A/AA, 2.1 A/AA, 2.2 AA.

### Requirement: Keyboard operation of core inputs
The suite SHALL verify keyboard focus and text entry for the recipe search and import URL; URL submission SHALL be reachable by Tab.

#### Scenario: Import URL without pointer
- **WHEN** the user focuses the URL entry, types a value, and presses Tab
- **THEN** the primary submit control receives focus and is enabled.
