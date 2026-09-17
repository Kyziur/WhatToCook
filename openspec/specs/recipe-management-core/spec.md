# recipe-management-core Specification

## Purpose

Defines the trusted recipe catalog lifecycle, normalized discovery, cook-friendly details, and deterministic starter data for the application.

## Requirements

### Requirement: User can create and maintain unique recipe records
The system SHALL allow the user to create, view, update, and archive recipe records containing a unique title, servings, ingredient list, preparation steps, tags, optional photo, and optional source needed for everyday cooking.

#### Scenario: Save a new recipe with complete core fields
- **WHEN** the user enters a unique title, servings, at least one ingredient, and at least one preparation step and chooses to save the recipe
- **THEN** the system stores the recipe and makes it available in the recipe library

#### Scenario: Edit an existing recipe
- **WHEN** the user opens an existing recipe, changes its content, and saves the update
- **THEN** the system persists the latest version and shows the updated details when the recipe is reopened

#### Scenario: Reject duplicate title
- **WHEN** the user tries to save a recipe whose title matches another recipe title after normalization
- **THEN** the system rejects the save and explains that recipe titles must be unique

#### Scenario: Archive a recipe
- **WHEN** the user archives an existing recipe
- **THEN** the system marks the recipe inactive and removes it from normal library and search results

### Requirement: User can attach recipe photo and source information
The system SHALL support one optional uploaded main photo and one optional source field whose value can be displayed either as plain text or as a clickable link when it is a URL.

#### Scenario: Save recipe with uploaded photo
- **WHEN** the user uploads one main photo while saving a recipe
- **THEN** the system stores the photo with the recipe and uses it in recipe listings and details

#### Scenario: Save recipe without photo
- **WHEN** the user saves a recipe without uploading a photo
- **THEN** the system stores the recipe and uses a placeholder image in list views

#### Scenario: Display source as clickable link
- **WHEN** the recipe source field contains a URL
- **THEN** the system renders the source as a clickable link

#### Scenario: Display source as plain text
- **WHEN** the recipe source field contains non-URL text
- **THEN** the system renders the source as plain text

### Requirement: User can structure recipes with normalized ingredients and free-form tags
The system SHALL allow the user to define recipe ingredients using a normalized ingredient reference, optional free-text quantity, optional controlled unit, and free-form tags normalized for matching and suggestions.

#### Scenario: Add ingredient from existing suggestion
- **WHEN** the user begins typing an ingredient that already exists in the catalog while editing a recipe
- **THEN** the system offers the normalized ingredient as a suggestion and allows the user to select it

#### Scenario: Add new normalized ingredient while editing
- **WHEN** the user enters an ingredient that does not yet exist in the ingredient catalog and saves the recipe
- **THEN** the system creates the normalized ingredient and associates it with the recipe

#### Scenario: Save ingredient without quantity
- **WHEN** the user saves a recipe ingredient with a normalized ingredient name but no quantity or unit
- **THEN** the system accepts the ingredient entry

#### Scenario: Suggest existing tag during editing
- **WHEN** the user begins typing a tag that already exists while editing a recipe
- **THEN** the system offers the normalized tag as a suggestion and allows the user to select it

### Requirement: User can browse and find active recipes quickly
The system SHALL provide a recipe library that supports alphabetical browsing, searching, and filtering of active recipes so the user can quickly locate a recipe to cook, edit, or plan.

#### Scenario: Search by title
- **WHEN** the user enters a search phrase matching a recipe title after case-insensitive and diacritic-insensitive normalization
- **THEN** the system narrows the visible recipe list to matching recipes

#### Scenario: Search by tag
- **WHEN** the user searches by one or more tags
- **THEN** the system narrows the visible recipe list to recipes matching those tags

#### Scenario: Search by all selected ingredients
- **WHEN** the user selects multiple normalized ingredients while searching
- **THEN** the system returns recipes containing at least all selected ingredients, even if the recipes contain additional ingredients

#### Scenario: Browse recipes alphabetically
- **WHEN** the user opens the recipe library without active search or filters
- **THEN** the system shows active recipes in alphabetical order by title

### Requirement: User can review a recipe in a cook-friendly format
The system SHALL present recipe details in a format that is easy to scan while cooking, with clear separation between ingredients, steps, and key metadata.

#### Scenario: Open recipe details from the library
- **WHEN** the user selects a recipe from the recipe library
- **THEN** the system shows a dedicated details view with the recipe title, metadata, ingredients, and steps

#### Scenario: Read steps sequentially while cooking
- **WHEN** the user views the preparation steps in the details screen
- **THEN** the system presents the steps in their saved order and keeps them visually distinct from the ingredient list

#### Scenario: Preserve edited step order
- **WHEN** the user reorders preparation steps in the recipe editor and saves the recipe
- **THEN** the system stores and displays the updated step order

### Requirement: Application can initialize a persistent starter recipe catalog
The system SHALL support startup seed data in the relational database so recipe browsing and details can run against persisted records instead of in-memory fake catalog implementations.

#### Scenario: Seed starter data into an empty catalog
- **WHEN** seed data is enabled and the recipe catalog is empty at startup
- **THEN** the system inserts starter recipes with structured ingredients, steps, and tags into the database

#### Scenario: Do not overwrite existing catalog data
- **WHEN** seed data is enabled and user recipes already exist in the catalog
- **THEN** the system skips seed insertion and preserves existing records unchanged
