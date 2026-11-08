## ADDED Requirements

### Requirement: Recipe Web client detects incompatible API response shapes

The separately deployed Web client SHALL reject recipe catalog and detail responses that omit mandatory fields or whose paging metadata does not match the request, rather than silently treating them as empty or default values.

#### Scenario: Required paging metadata is missing
- **WHEN** the API returns a recipe page missing `totalCount` or a mandatory recipe item field
- **THEN** the Web client fails deserialization instead of rendering empty totals or blank recipe cards

#### Scenario: Server sends a different page
- **WHEN** the API response includes `page` or `pageSize` different from the requested values
- **THEN** the Web client fails the operation instead of rendering the wrong page

#### Scenario: Required detail content is missing
- **WHEN** a recipe details response omits mandatory instructions or ingredient fields
- **THEN** the Web client rejects the incompatible response rather than hiding recipe content

#### Scenario: Real API continues supplying required fields
- **WHEN** a recipe is created and fetched via paged catalog and details HTTP endpoints
- **THEN** the endpoints return required JSON names, types and page metadata for the Web consumer, with optional fields remaining optional
