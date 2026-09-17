## Purpose

Keeps recipe discovery, planning, and creation efficient across phone, tablet, and desktop layouts without changing the application's simple three-destination mental model.

## ADDED Requirements

### Requirement: Core screens fit phone viewports and keep primary actions reachable
The system SHALL render core workflows without horizontal page scrolling at a 375 CSS-pixel viewport and SHALL keep the current workflow's primary action reachable without returning to the end of a long page.

#### Scenario: Use the application shell on a phone
- **WHEN** the user opens a core screen at a 375 CSS-pixel viewport
- **THEN** the shell, content, and navigation fit the viewport and expose touch targets at least 44 CSS pixels high

#### Scenario: Edit a long form or plan on a phone
- **WHEN** the user scrolls a recipe editor or selected meal plan
- **THEN** the relevant save action remains available above the primary navigation without obscuring content

### Requirement: Recipe discovery starts with title search
The system SHALL present title search before advanced tag and ingredient filters, SHALL keep selected advanced filters visible when the filter controls are collapsed, and SHALL keep a discovery control reachable while long result sets are being read.

#### Scenario: Open the library on a phone
- **WHEN** the user opens the recipe library at a phone-sized viewport
- **THEN** title search and recipe results appear before the expanded advanced-filter controls

#### Scenario: Apply advanced filters
- **WHEN** the user selects tag or ingredient filters and closes the responsive filter panel
- **THEN** the library shows the selected filters, result count, and a clear action without losing URL-backed filter state

#### Scenario: Combine ingredient filters
- **WHEN** the user selects multiple ingredient checkboxes
- **THEN** the user can switch between recipes containing all selected ingredients and recipes containing any selected ingredient

#### Scenario: Browse a long recipe library
- **WHEN** the user scrolls through a large recipe result set
- **THEN** title search remains reachable without hiding primary navigation

### Requirement: Ingredient filters represent product names
The system SHALL expose ingredient filters as product names without embedded quantities or recognized units and SHALL match recipes whose legacy ingredient text includes such a prefix.

#### Scenario: Browse an imported ingredient that contains a quantity and unit
- **WHEN** a recipe ingredient is stored as text such as `120g napoju owsianego`
- **THEN** the filter displays `napoju owsianego` and selecting it finds that recipe

### Requirement: Primary navigation remains reachable on long desktop and phone pages
The system SHALL expose the three primary destinations independently of document length on desktop and phone layouts.

#### Scenario: Browse a long result set on a desktop
- **WHEN** the user scrolls far below the initial viewport in a large recipe library
- **THEN** the user can navigate to Biblioteka, Planer, or Zakupy without first returning to the page footer

### Requirement: Large development data supports visual scale checks
The system SHALL offer an opt-in, deterministic development seed profile with 100 active recipes and 20 meal plans so responsive browsing and plan selection can be inspected at realistic scale.

#### Scenario: Start the API with the large development seed profile
- **WHEN** development seed profile `large` is enabled against an empty database
- **THEN** the API seeds exactly 100 active recipes and 20 meal plans with valid plan entries

### Requirement: Recipe browsing supports direct plan assignment
The system SHALL let the user assign an active recipe to an existing plan day from library or detail context without navigating through the planner first.

#### Scenario: Add a recipe to an eligible plan day
- **WHEN** the user chooses an existing plan and day from the direct assignment control and confirms
- **THEN** the recipe is added once using the default multiplier and the user receives success feedback without leaving browsing context

#### Scenario: Select a day that already contains the recipe
- **WHEN** the chosen day already contains the selected recipe
- **THEN** the system blocks duplicate assignment and explains how to choose another day

### Requirement: Meal planning prioritizes the selected plan and day
The system SHALL collapse plan creation when plans already exist, SHALL avoid repeating inactive add controls for every day on a phone, and SHALL preserve the simple day-list model.

#### Scenario: Open a planner that contains plans
- **WHEN** the planner finishes loading at least one plan
- **THEN** the system selects a plan, presents its compact summary, and keeps new-plan creation collapsed

#### Scenario: Navigate a weekly plan on a phone
- **WHEN** the user selects a day from the plan's day picker
- **THEN** the system shows that day's recipes and add controls without rendering every day's full editor in sequence

#### Scenario: Review a plan on a desktop
- **WHEN** the viewport has room for multiple day summaries
- **THEN** the system displays compact day cards while preserving access to the selected day's editing controls
