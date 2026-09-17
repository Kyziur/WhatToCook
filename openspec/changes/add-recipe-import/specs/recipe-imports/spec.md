## ADDED Requirements

### Requirement: User can create a recipe import draft from a recipe URL
The system SHALL allow the user to submit a supported recipe URL and create a persisted import draft containing extracted recipe fields and source metadata without writing directly into the primary recipe tables.

#### Scenario: Create import draft from supported recipe page
- **WHEN** the user submits a supported recipe URL and the extractor can read the recipe page
- **THEN** the system creates an import draft containing the extracted title, source URL, ingredients, and steps

#### Scenario: Reject invalid import URL
- **WHEN** the user submits an invalid or non-HTTP URL for import
- **THEN** the system rejects the request and explains that a valid recipe URL is required

#### Scenario: Report unsupported or unreadable source
- **WHEN** the user submits a URL that cannot be extracted into a recipe draft
- **THEN** the system returns an actionable import error instead of creating a partial trusted recipe

### Requirement: User can review and correct imported recipe draft fields before saving
The system SHALL persist imported recipe data as a reviewable draft that can be reopened, corrected, and evaluated for finalize blockers before the user creates a real recipe.

#### Scenario: Reopen draft with extracted fields and issues
- **WHEN** the user requests an existing import draft
- **THEN** the system returns the extracted recipe fields, source metadata, and any draft issues that require review before finalization

#### Scenario: Correct imported draft before finalization
- **WHEN** the user updates title, servings, ingredients, steps, source, or tags in an import draft
- **THEN** the system persists the corrected draft values without yet creating a recipe record

#### Scenario: Merge imported steps during review
- **WHEN** the importer splits one preparation instruction into multiple consecutive draft steps and the user chooses to merge a step with the one above it
- **THEN** the system combines both step texts into the earlier step, preserves their relative order, and removes the merged step from the draft

### Requirement: User can finalize an import draft into a normal recipe
The system SHALL create a normal recipe from an import draft only after the draft satisfies recipe validation and no finalize blockers remain.

#### Scenario: Finalize ready draft into recipe
- **WHEN** the user finalizes an import draft that contains valid recipe data
- **THEN** the system creates a recipe in the primary recipe tables and marks the draft finalized

#### Scenario: Block finalize on duplicate normalized title
- **WHEN** the user tries to finalize an import draft whose title conflicts with an existing recipe after normalization
- **THEN** the system rejects finalization and leaves the draft available for correction

#### Scenario: Block finalize on missing required recipe fields
- **WHEN** the user tries to finalize an import draft that is missing required recipe data such as servings, ingredients, or steps
- **THEN** the system rejects finalization and identifies the remaining blockers

### Requirement: User can prepare photo-based recipe imports through the same draft workflow
The system SHALL support recipe import drafts created from photographed recipe pages using the same review and finalization workflow as URL imports.

#### Scenario: Create draft from photographed recipe pages
- **WHEN** the user uploads one or more photographed recipe pages for import
- **THEN** the system creates a persisted import draft source that can later be extracted and reviewed before finalization

#### Scenario: Persist raw extraction artifacts for photo imports
- **WHEN** the system extracts recipe content from photographed pages
- **THEN** the system stores the intermediate source artifacts needed to support review, correction, and future extractor improvements
