## ADDED Requirements

### Requirement: Critical interactive controls respond to keyboard input

The system SHALL expose usable keyboard activation and state feedback in the core library, editor, and shopping-list flows.

#### Scenario: Mobile filters announce their state
- **WHEN** a keyboard user activates the library filter disclosure using Enter or Space
- **THEN** the visible panel state and the control's aria-expanded state change consistently

#### Scenario: Editor keyboard order
- **WHEN** the user tabs from the recipe title
- **THEN** focus reaches the serving-count input without an unrelated stop

#### Scenario: Shopping checklist keyboard operation
- **WHEN** a keyboard user activates an existing checklist item using Space or Enter
- **THEN** its persisted MAM/NIE_MAM state and aria-pressed state change as with pointer input

#### Scenario: Shopping list updates do not strand keyboard focus
- **WHEN** a focused checklist item changes between MAM and NIE_MAM and changes its display position
- **THEN** that same item's control retains focus and rejects extra mutations during pending saves
