## ADDED Requirements

### Requirement: Retrying a failed photo upload after new recipe creation does not create a second recipe
The recipe editor SHALL remember the successfully created recipe's ID in its current editor session before attempting the optional image upload. If that image upload fails, a subsequent save SHALL update the already created recipe and retry its image upload instead of issuing another recipe-create request.

#### Scenario: Photo upload returns a failure
- **WHEN** a new recipe is persisted and its photo upload reports failure
- **THEN** the editor leaves the pending photo available and offers a retry
- **AND** the next save uses the persisted recipe ID to update the recipe before uploading the photo again

#### Scenario: Photo upload throws after create
- **WHEN** the photo upload raises an error after a successful recipe create response
- **THEN** a subsequent save in the same editor session updates that recipe rather than creating another one

#### Scenario: Create without photo succeeds
- **WHEN** a new recipe is saved successfully without a selected photo
- **THEN** the editor continues to the newly created recipe's details using its returned ID
