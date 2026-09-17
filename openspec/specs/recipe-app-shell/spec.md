# recipe-app-shell Specification

## Purpose

Defines the primary navigation, context preservation, first-use states, and mobile-first shell behavior for the recipe application.

## Requirements

### Requirement: Application exposes a clear primary navigation model
The system SHALL provide a primary navigation structure that makes the recipe library, meal planner, shopping lists, recipe details, and recipe creation or editing flows consistently reachable.

#### Scenario: Enter the application after launch
- **WHEN** the user opens the application in a browser during a normal signed-out-free personal usage flow
- **THEN** the system lands in the recipe library as the primary entry screen

#### Scenario: Start creating a recipe from the main shell
- **WHEN** the user activates the primary create action from the application shell
- **THEN** the system opens the recipe editor without requiring the user to leave the main experience

#### Scenario: Open planner from the main shell
- **WHEN** the user navigates to meal planning from the primary shell
- **THEN** the system opens the meal plan experience without exposing inactive recipes as normal library items

### Requirement: Application preserves user context between list, detail, and edit flows
The system SHALL preserve user context so the user can move between the recipe list, details, editor, planner, and shopping flows without losing orientation or unsaved work unexpectedly.

#### Scenario: Return from details to the library
- **WHEN** the user opens a recipe from the library and then navigates back
- **THEN** the system returns the user to the library with their previous browsing context intact

#### Scenario: Attempt to leave the editor with unsaved changes
- **WHEN** the user has unsaved changes in the recipe editor and tries to navigate away
- **THEN** the system warns the user before discarding those changes

#### Scenario: Attempt to save a changed plan with an existing shopping list
- **WHEN** the user modifies a meal plan that already has a shopping list and tries to save the plan
- **THEN** the system requires the user to choose between regeneration or canceling the save

### Requirement: Application communicates empty and first-use states clearly
The system SHALL provide explicit empty states so a new user understands what to do when the library has no recipes or a search returns no results.

#### Scenario: Open the application with an empty library
- **WHEN** the user enters the recipe library and no recipes exist yet
- **THEN** the system shows an empty-state message and a clear path to create the first recipe

#### Scenario: Search returns no recipes
- **WHEN** the user performs a search or applies filters that match no recipes
- **THEN** the system shows a no-results state and preserves access to clear the search or filters

#### Scenario: Open planner with no meal plans
- **WHEN** the user opens the planner area and no meal plans exist yet
- **THEN** the system shows an empty-state message and a clear path to create the first plan

### Requirement: Application shell supports mobile-first ergonomics
The system SHALL prioritize a mobile-first interaction model with layouts and actions that remain usable on phone-sized screens while still adapting cleanly to larger surfaces.

#### Scenario: View the primary shell on a phone-sized screen
- **WHEN** the user opens the application in a mobile browser on a narrow screen
- **THEN** the system keeps primary actions and core navigation reachable without horizontal scrolling

#### Scenario: View the primary shell on a larger screen
- **WHEN** the user opens the application on a tablet or desktop-sized layout
- **THEN** the system expands content density or panel layout without changing the core navigation concepts
