## 1. Solution Bootstrap

- [x] 1.1 Scaffold the Blazor Web App frontend project under `src/` with Interactive Server as the default MVP render mode and add it to the solution
- [x] 1.2 Scaffold the ASP.NET Core minimal API backend project under `src/` and add it to the solution
- [x] 1.3 Add Aspire AppHost and shared defaults so Blazor Web App, backend, and PostgreSQL run together locally
- [x] 1.4 Establish frontend and backend feature-slice structure for recipes, planning, and shopping flows
- [x] 1.5 Define shared design tokens, typography, color usage, and responsive layout primitives for the Blazor frontend

## 2. Recipe Persistence Foundation

- [x] 2.1 Define the recipe domain model and persistence entities for unique title, servings, normalized ingredients, steps, tags, source, and active status
- [x] 2.2 Add file upload handling that creates a single image directory on disk and stores only the file path reference in the database
- [x] 2.3 Add EF Core persistence with PostgreSQL and a clear minimal API boundary for recipe CRUD and archive operations
- [x] 2.4 Add behavior-focused integration tests covering recipe persistence rules, title uniqueness, normalized matching, and CRUD outcomes
- [x] 2.5 Add startup seed data for empty recipe catalogs so the frontend no longer relies on in-memory fake recipe lists
- [x] 2.6 Migrate integration tests to PostgreSQL Testcontainers and keep capability-level business spec test traceability

## 3. Blazor Web App Use Cases and Navigation

- [x] 3.1 Implement the Blazor Web App shell so the application opens into the recipe library
- [x] 3.2 Implement navigation between library, details, editor, planner, and shopping flows while preserving user context
- [x] 3.3 Implement empty states, validation feedback, and plan-save regeneration prompts in the relevant frontend flows using Interactive Server by default
- [x] 3.4 Implement mobile-first navigation and sticky primary actions for phone layouts

## 4. Recipe Library and Details Experience

- [x] 4.1 Implement the recipe library screen with alphabetical ordering and empty-state behavior
- [x] 4.2 Implement search and filter behavior for title, tags, and all selected ingredients using normalized matching
- [ ] 4.2a Define metadata-filter contract and implementation scope (PENDING; owner: `design-recipe-metadata-filter` task 1.1)
- [x] 4.3 Implement the recipe details screen with cook-friendly presentation of servings, source, ingredients, steps, and photo or placeholder
- [x] 4.4 Apply the approved warm utility visual direction to recipe cards, chips, placeholders, and metadata hierarchy
- [x] 4.5 Add integration and end-to-end tests covering library filtering, empty states, detail presentation, and key story outcomes

## 5. Recipe Editor Experience

- [x] 5.1 Implement recipe creation with validation for required title, servings, ingredient, and step fields
- [x] 5.2 Implement recipe editing with ingredient suggestions, free-form tag suggestions, and manual step reordering
- [x] 5.3 Implement archive flows from the library or details experience instead of hard delete
- [x] 5.4 Implement the editor as a visually guided workflow with clear sectioning for basics, ingredients, steps, media, and source
- [x] 5.5 Add integration and end-to-end tests covering editor validation, save behavior, normalized suggestion behavior, and archive handling

## 6. Meal Planning

- [x] 6.1 Implement meal plan creation with editable default name and arbitrary date range
- [x] 6.2 Implement day-level planned recipe assignment with one occurrence per recipe per day, manual ordering, and positive numeric multiplier
- [x] 6.3 Exclude inactive recipes from new plan selection while preserving them in existing plans
- [x] 6.4 Implement planner layouts as simple day-based lists rather than dense calendar-style views
- [x] 6.5 Add integration and end-to-end tests covering overlap support, per-day uniqueness, ordering, multiplier validation, and save-time regeneration prompts

## 7. Shopping List Management

- [x] 7.1 Implement on-demand shopping list generation from a meal plan with aggregation by normalized ingredient and unit
- [x] 7.2 Implement shopping item checklist state, manual text items, and plain-text copy action
- [x] 7.3 Implement save-time regeneration decision and non-destructive generated-item reconciliation that preserves matching checklist state and manual items
- [x] 7.4 Implement shopping checklist interactions with large tap targets and visually distinct `MAM` state treatment
- [x] 7.5 Add integration and end-to-end tests covering aggregation rules, regeneration behavior, manual items, and plain-text export

## 8. Self-Hosted Platform and Deployment

- [x] 8.1 Add Linux-friendly runtime configuration for ports, connection strings, upload storage paths, and data persistence paths
- [x] 8.2 Add Docker-based deployment assets suitable for Raspberry Pi hosting
- [x] 8.3 Document the Komodo/Bitwarden self-hosted deployment contract with required `DATA_ROOT` and `POSTGRES_PASSWORD` (no embedded production credentials), Interactive Server render-mode assumptions, and Aspire-based local development; deployment orchestration and backup/restore data operations are owned by the homelab repository
