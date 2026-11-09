## ADDED Requirements

### Requirement: Editor data belongs to its current recipe route
The editor SHALL reset the form to defaults when switching from an existing recipe to the new-recipe route, and SHALL not carry a pending photo into editing another recipe.

#### Scenario: Transition from editing to creating
- **WHEN** the editor component is reused for the new-recipe route after loading a recipe
- **THEN** the form contains the default empty new recipe
- **AND** saving uses a create request, not the previous recipe ID

#### Scenario: Switching edited recipes with a pending photo
- **WHEN** the editor moves to a different recipe ID after a local photo was selected
- **THEN** the selected photo from the previous recipe is cleared
- **AND** saving the target recipe does not upload that old photo

#### Scenario: Target recipe cannot be loaded
- **WHEN** loading a different recipe ID fails or the recipe is missing
- **THEN** the previous recipe cannot be saved as the requested target recipe
