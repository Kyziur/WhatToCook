## ADDED Requirements

### Requirement: User can create named meal plans for any date range
The system SHALL allow the user to create a meal plan with a date range and editable name so that household members can organize recipes for a selected period.

#### Scenario: Create a plan with a default name
- **WHEN** the user creates a new meal plan for a chosen date range
- **THEN** the system assigns a default name derived from the selected date range and allows the user to change it

#### Scenario: Create overlapping plans
- **WHEN** the user creates a new meal plan whose date range overlaps another existing plan
- **THEN** the system stores the new plan as a separate valid plan

### Requirement: User can assign recipes to specific days in a meal plan
The system SHALL allow the user to add active recipes to individual days in a meal plan so that the same recipe can be planned on multiple days while appearing at most once per day in the same plan.

#### Scenario: Add a recipe to a plan day
- **WHEN** the user selects an active recipe while editing a meal plan day
- **THEN** the system adds the recipe to that day in the plan

#### Scenario: Prevent duplicate recipe on the same day
- **WHEN** the user tries to add the same recipe to the same day twice in the same plan
- **THEN** the system rejects the duplicate entry for that day

#### Scenario: Reuse a recipe on another day
- **WHEN** the user adds the same recipe to a different day in the same meal plan
- **THEN** the system accepts the entry as a separate planned recipe

### Requirement: Planned recipes preserve order and multiplier
The system SHALL allow the user to manually reorder planned recipes within a day and set a positive numeric multiplier that scales the recipe for planning and shopping purposes.

#### Scenario: Reorder planned recipes
- **WHEN** the user manually rearranges recipes within the same day
- **THEN** the system saves and displays the updated order

#### Scenario: Set recipe multiplier
- **WHEN** the user assigns a positive numeric multiplier such as `0.5`, `1`, `1.5`, or `2` to a planned recipe
- **THEN** the system stores the multiplier and uses it when generating the shopping list

### Requirement: Existing plans keep references to inactive recipes
The system SHALL preserve already planned recipes even after the underlying recipe becomes inactive so that historical or in-progress plans remain readable.

#### Scenario: View a plan after a recipe becomes inactive
- **WHEN** a recipe that is already present in a meal plan is later marked inactive
- **THEN** the existing meal plan still displays that recipe entry

#### Scenario: Add inactive recipe to a new plan day
- **WHEN** the user searches for recipes while editing a plan and the recipe is inactive
- **THEN** the system excludes that recipe from normal plan selection
