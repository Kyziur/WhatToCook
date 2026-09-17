## Purpose

Defines a responsive cooking workspace that makes weekly planning, recipe assignment, and shopping state understandable at a glance while preserving existing planning behavior.

## ADDED Requirements

### Requirement: Desktop planning presents a weekly workspace
The system SHALL present the selected plan as a seven-day dashboard with stable meal-slot rows, summary metrics, and contextual shopping information on wide viewports.

#### Scenario: Open a weekly plan on desktop
- **WHEN** a selected plan contains at least seven consecutive days
- **THEN** the interface displays day columns, breakfast/second-breakfast/dinner/supper rows, assigned recipe cards, and empty add targets without horizontal page scrolling

#### Scenario: Review plan progress
- **WHEN** the weekly workspace is visible
- **THEN** the user can see planned meal count, planned day progress, shopping item count, plan date range, dirty state, and save/generate actions without reaching the page end

### Requirement: Mobile planning focuses one day at a time
The system SHALL show a compact day selector and one selected day's meal slots on phone-sized viewports.

#### Scenario: Select another day on a phone
- **WHEN** the user activates a day in the horizontal week selector
- **THEN** the four meal slots update for that day while navigation and the add action remain thumb-reachable

### Requirement: Meal slots persist through existing ordering semantics
The system SHALL assign planned recipes to breakfast, second breakfast, dinner, or supper and preserve that assignment through save and reload without changing the API schema.

#### Scenario: Add a recipe to an empty meal slot
- **WHEN** the user activates an empty slot, selects a recipe, and confirms
- **THEN** the recipe appears in that day and slot, remains unique for the day, and is persisted with its multiplier and relative order

#### Scenario: Load a legacy entry without encoded slot information
- **WHEN** an existing plan entry uses the legacy sequential sort order
- **THEN** the interface assigns it to dinner and persists the normalized slot order on the next save

### Requirement: Recipe selection is contextual and responsive
The system SHALL open recipe search in a desktop side drawer or mobile sheet from a selected day and slot.

#### Scenario: Open the recipe picker
- **WHEN** the user activates an add target
- **THEN** the picker identifies the destination day and meal slot, supports recipe search, blocks duplicates, and returns focus context after adding or cancelling

### Requirement: Shopping context accompanies planning
The system SHALL show the selected plan's generated shopping items beside the desktop planner and keep the standalone shopping route optimized for checklist use on phones.

#### Scenario: Review shopping while planning on desktop
- **WHEN** the selected plan has a generated shopping list
- **THEN** a scrollable side panel shows item states and provides access to the full shopping route

#### Scenario: Open shopping on a phone
- **WHEN** the user navigates to the shopping route
- **THEN** the page prioritizes checklist rows, compact plan context, generation/copy actions, and a thumb-reachable manual-add action

### Requirement: Recipe details support cooking and planning contexts
The system SHALL present recipe media, ingredients, steps, and add-to-plan action in a responsive hierarchy aligned with the planning workspace.

#### Scenario: Open recipe details on a phone
- **WHEN** a recipe is loaded on a phone-sized viewport
- **THEN** the image leads the page, ingredients and steps remain readable, and add-to-plan stays reachable near the bottom safe area
