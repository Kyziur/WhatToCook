## ADDED Requirements

### Requirement: Recipe editors provide accessible tag removal
Both the recipe editor and imported-draft review SHALL show removable tags with an accessible name identifying the tag. Removing a tag SHALL update only the owning workflow's tag selection.

#### Scenario: Removing a tag from the editor or import-review form
- **WHEN** the user activates a tag's remove control
- **THEN** the matching tag is removed by the current page
- **AND** the control is announced as removing that tag
