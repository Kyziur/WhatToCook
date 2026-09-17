## Context

The repository is an empty .NET application template with OpenSpec already configured and no active product code in `src/` or `tests/`. This change defines the first real product direction for a personal recipe management application, so the design needs to balance speed of delivery with enough structure to avoid repainting the house after the first few screens.

Key constraints:
- There is no existing domain model or client shell to extend.
- The application will be a web application with a database, hosted on Linux on Raspberry Pi, initially available only in the local network.
- The repository stack naturally supports ASP.NET Core and pragmatic testable patterns.
- The preferred interaction model is a Blazor Web App, with Interactive Server as the default MVP render mode and the option to introduce client-side rendering selectively later.
- The UI must be mobile-first and work well on phones while remaining usable on larger screens.
- The MVP problem is primarily household recipe management, not collaboration or public internet scale.

Stakeholders:
- Product owner defining the first usable slice
- Future implementation agent that will translate the spec into code
- Future reviewers who will need clear scope boundaries and testable behavior

## Goals / Non-Goals

**Goals:**
- Establish an MVP focused on recipe CRUD, organization, discovery, meal planning, shopping list generation, and a clear application shell.
- Choose a web architecture that is easy to host on Raspberry Pi and easy to extend as the product grows.
- Define a UI structure that works well on phones first and scales to larger screens.
- Keep local development friction low by orchestrating frontend, backend, and database together.
- Keep the solution open to future authentication, import/export, and broader network access without paying that complexity cost up front.

**Non-Goals:**
- Multi-user collaboration, sharing, ratings, or social features
- Pantry inventory, automatic unit conversion, recipe scraping, OCR import, and AI extraction workflows
- Public internet exposure, cloud synchronization, and complex access control in the first slice
- Premature extraction into many projects or generic abstractions before the first feature set exists

## UX/UI Direction

### Visual Direction

The application should feel like a calm household utility for cooking, not a social food platform or an enterprise dashboard. The visual tone should be warm, tactile, and readable, with enough personality to feel intentional while staying efficient during daily kitchen use.

Recommended visual style:
- Warm neutral backgrounds rather than stark white
- Soft card surfaces with clear visual grouping
- Terracotta as the main action color
- Herb green reserved for positive checklist and success states
- Strong text contrast and low visual noise
- Consistent placeholder imagery instead of random stock visuals

### Design Tokens

Suggested MVP token set:

- `color-bg`: `#F7F3EA`
- `color-surface`: `#FFFDF8`
- `color-surface-alt`: `#F1EADF`
- `color-text`: `#1F2937`
- `color-text-muted`: `#6B7280`
- `color-border`: `#D9CDBB`
- `color-primary`: `#C65D2E`
- `color-primary-hover`: `#A94C24`
- `color-primary-soft`: `#F3C3A8`
- `color-success`: `#5E8B4A`
- `color-success-soft`: `#DDE9D5`
- `color-warning`: `#B7791F`
- `color-danger`: `#A63D40`
- `color-focus`: `#2F6FED`

Semantic usage rules:
- Use `color-primary` for the single dominant CTA on a screen.
- Use `color-success` only for positive checklist or completed states such as `MAM`.
- Use muted or neutral tones for tags, chips, and secondary actions.
- Keep destructive or archive actions visually separated from the main CTA.

### Typography

Recommended pairing:
- Headings: `Fraunces`
- Body and UI text: `Manrope`

Type principles:
- Headings should feel slightly editorial, like a modern recipe notebook.
- Body text should stay highly legible on small screens.
- Step text, ingredient lists, and shopping items should prioritize clarity over visual flourish.

### UX Principles

Core UX rules for MVP:
- Mobile-first is mandatory, not aspirational.
- Each screen should have one clearly dominant next action.
- Important actions should stay thumb-reachable on phones.
- Filters should use chips, toggles, or lightweight selectors instead of heavy forms.
- Reading and cooking flows should minimize cognitive load and visual clutter.
- Planner and shopping interactions should favor speed over dense information display.
- Validation and save states should be explicit and calm rather than noisy.

Interaction patterns:
- Use a sticky bottom action bar on phone layouts for primary actions such as `Dodaj przepis`, `Zapisz plan`, or `Generuj liste zakupow`.
- Use chips for tags, ingredient filters, and quick state indicators.
- Use large tap targets for shopping checklist interactions.
- Prefer inline confirmations and lightweight dialogs over navigation-heavy confirmation pages.
- Keep archive actions behind a secondary affordance to reduce accidental taps.

### Screen Layout Guidance

**1. Recipe Library**
- Layout should lead with a search field and lightweight filter chips.
- Recipe entries should appear as compact cards with image, title, tags, and preparation cues.
- The create action should always be easy to reach on mobile.

```text
+------------------------------------------------+
| Search recipes...                              |
| [tag] [ingredient] [time]                      |
+------------------------------------------------+
| [photo] Greek Salad                            |
|         szybkie 15min                          |
+------------------------------------------------+
| [photo] Tomato Soup                            |
|         obiad klasyk                           |
+------------------------------------------------+
|                 [ Dodaj przepis ]              |
+------------------------------------------------+
```

**2. Recipe Details**
- Prioritize fast scanning: title, metadata, ingredients, then steps.
- Ingredient and step sections should be visually distinct.
- The source field should sit low in the hierarchy and never compete with cooking content.

```text
+------------------------------------------------+
| Greek Salad                                    |
| 4 porcje  15 min                               |
| [photo / placeholder]                          |
| Ingredients                                    |
| - 2 pomidory                                   |
| - 1 ogorek                                     |
| - feta                                         |
| Steps                                          |
| 1. Pokroj warzywa                              |
| 2. Dodaj fete                                  |
| 3. Wymieszaj                                   |
| [ Edytuj ]                                     |
+------------------------------------------------+
```

**3. Recipe Editor**
- The editor should feel like a guided workflow even if implemented as one page.
- Suggested structure: basics, ingredients, steps, media, source.
- Save action should stay consistently reachable.

```text
+------------------------------------------------+
| Title                                          |
| Servings                                       |
| Tags                                           |
| Ingredients [+ add row]                        |
| Steps       [drag/reorder]                     |
| Photo upload                                   |
| Source                                         |
|                 [ Zapisz przepis ]             |
+------------------------------------------------+
```

**4. Meal Planner**
- Show one selected date range at a time with a clear title.
- Each day should read like a simple list, not a heavy calendar widget.
- Reordering should be obvious and lightweight.

```text
+------------------------------------------------+
| Plan tydzien 28.04 - 04.05                     |
| Monday                                         |
| 1. Greek Salad        x1                       |
| 2. Pizza              x2                       |
| Tuesday                                        |
| 1. Tomato Soup        x1                       |
|                 [ Generuj liste zakupow ]      |
+------------------------------------------------+
```

**5. Shopping List**
- This is the most utility-focused screen in the app.
- Checklist interactions should dominate over decoration.
- Copy action should be obvious and instant.

```text
+------------------------------------------------+
| Lista zakupow - Plan tydzien 28.04 - 04.05     |
| [ Kopiuj ]                                     |
| [ ] 5 pomidorow                                |
| [x] feta                                       |
| [ ] 2 ogorki                                   |
| [ ] papier do pieczenia                        |
+------------------------------------------------+
```

### Responsive Behavior

Phone-first rules:
- Single-column layouts by default
- Sticky primary actions
- Minimal horizontal grouping
- Compact metadata rows that wrap cleanly

Larger-screen enhancements:
- Recipe library can become a denser card grid
- Details can move to a two-column content layout
- Planner can show more days at once without changing the mental model
- Shopping list can introduce a secondary panel for copy preview or plan summary if useful later

## Domain Model

### Aggregates and Entities

**Recipe**
- Aggregate root representing one recipe in the household catalog.
- Core fields: `Id`, `Title`, `Servings`, `Source`, `MainPhotoPath`, `IsActive`, `CreatedAt`, `UpdatedAt`.
- Owns `RecipeIngredient` and `RecipeStep` collections.
- Relates to `Tag` through a normalized many-to-many association.

**RecipeIngredient**
- Child entity inside `Recipe`.
- Fields: `Id`, `IngredientId`, `QuantityText`, `Unit`, `SortOrder`.
- `QuantityText` remains free text for user convenience, while `IngredientId` points to a normalized ingredient for searching and shopping aggregation.
- `Unit` comes from a controlled list: `szt`, `g`, `kg`, `ml`, `l`, `łyżeczka`, `łyżka`, `szklanka`, `opakowanie`, or no unit.

**RecipeStep**
- Child entity inside `Recipe`.
- Fields: `Id`, `Text`, `SortOrder`.
- Supports manual reordering in the editor.

**Ingredient**
- Catalog entity representing a normalized ingredient such as `pomidor`, `cebula czerwona`, or `feta`.
- Fields: `Id`, `DisplayName`, `NormalizedName`.
- Shared across recipes to support ingredient-based search and shopping list aggregation.

**Tag**
- Catalog entity representing a free-form label such as `szybkie` or `15min`.
- Fields: `Id`, `DisplayName`, `NormalizedName`.
- Suggested during editing but remains user-defined and non-hierarchical.

**MealPlan**
- Aggregate root representing a named date-range plan.
- Fields: `Id`, `Name`, `DateFrom`, `DateTo`, `CreatedAt`, `UpdatedAt`.
- Owns `PlannedRecipe` entries.
- May overlap other meal plans.

**PlannedRecipe**
- Child entity inside `MealPlan`.
- Fields: `Id`, `RecipeId`, `PlannedDate`, `Multiplier`, `SortOrder`.
- The same recipe may appear on multiple days in the same plan but at most once per day.

**ShoppingList**
- Aggregate root generated from one meal plan on demand.
- Fields: `Id`, `MealPlanId`, `CreatedAt`, `UpdatedAt`.
- Owns `ShoppingListItem` entries.
- Is reconciled after user confirmation when the meal plan changes, preserving matching generated state and manual items, including duplicates, while removing stale generated entries.

**ShoppingListItem**
- Child entity inside `ShoppingList`.
- Fields for generated items: `IngredientDisplayName`, `NormalizedIngredientName`, `Unit`, `AggregatedQuantityText`, `State`, `IsManual`.
- Generated items aggregate by normalized ingredient plus unit.
- Manual items remain plain text and are not normalized.

### Value Objects and Supporting Concepts

**NormalizedText**
- Used conceptually for title uniqueness, tag matching, and ingredient matching.
- Case-insensitive and diacritic-insensitive.

**IngredientUnit**
- Controlled unit set used in recipes and generated shopping items.

**ChecklistState**
- Two-state shopping value: `MAM` or `NIE_MAM`.

### Domain Invariants

- Recipe title is required and unique after normalization.
- Recipe servings are required.
- Recipe must have at least one ingredient and at least one step.
- Recipe ingredients preserve entry order; recipe steps preserve editable order.
- Ingredient reference is required for every recipe ingredient, but quantity and unit are optional.
- Source is optional and stored as a single field rendered either as URL or plain text.
- Main photo is optional; missing photo falls back to a placeholder at the presentation layer.
- Main photo storage is filesystem-backed, with the database storing only the file path reference.
- Archived recipes are marked inactive, excluded from normal library and search results, but remain visible in existing meal plans.
- Meal plan requires a name and valid date range.
- Planned recipe multiplier must be a positive numeric value.
- Within a single meal plan day, the same recipe can appear at most once.
- Shopping list exists only after explicit generation.
- Shopping list regeneration preserves matching generated state and manual items, including duplicates, and removes stale generated entries.

## Primary Use Cases

### 1. Browse recipe library
- User opens the home page and sees active recipes in alphabetical order.

### 2. Search and filter recipes
- User searches by title in a case-insensitive, diacritic-insensitive way.
- User filters by tags or by selected ingredients that all must be present in the recipe.

### 3. View recipe details
- User opens a recipe to read ingredients and steps in a cooking-friendly layout.

### 4. Create a recipe
- User enters title, servings, source, tags, ingredients, and steps and optionally uploads a main photo.

### 5. Edit an existing recipe
- User updates recipe content and saves changes without losing ingredient entry order or custom step order.

### 6. Archive a recipe
- User marks a recipe inactive so it disappears from normal browsing but survives in existing plans.

### 7. Create and maintain a meal plan
- User creates a named plan for any date range.
- User adds recipes to days, reorders them, and sets a multiplier.

### 8. Generate and use a shopping list
- User explicitly generates a shopping list from a plan.
- User marks items as `MAM` or `NIE MAM`, adds manual items, and copies the list as plain text.

### 9. Access the app from another device on the local network
- User opens the app URL from a phone or another household device and uses the same web application without installing anything.

## Decisions

### 1. Build the product as a Blazor Web App plus ASP.NET Core backend

Decision:
- The first implementation should use a Blazor Web App frontend that talks to an ASP.NET Core backend over HTTP, with both parts deployable in a Linux-friendly environment suitable for Raspberry Pi.

Rationale:
- Browser-based access is the cleanest way to support phones and other household devices without client installation.
- Blazor Web App supports both server-side and client-side rendering, which gives flexibility for the future.
- For MVP, Interactive Server is the best default because it keeps the solution simpler than adding a WebAssembly client path up front while still providing rich interactivity for recipe editing, planning, and shopping workflows.
- ASP.NET Core aligns with the repository's strengths and deploys cleanly on Linux and ARM-friendly container environments.

Alternatives considered:
- .NET MAUI app: good mobile UX, but directly conflicts with the requirement for browser-based LAN access.
- Server-rendered MVC or Razor UI: simpler initially, but not aligned with the preferred component-based Blazor interaction model.

Implications:
- Initial code should start with at least two product projects under `src/`: Blazor Web App frontend and backend API.
- Frontend routing, component state, and HTTP contracts become first-class concerns.
- MVP render mode should default to Interactive Server for the main application shell and feature routes.

### 2. Use a modular monolith backend with feature slices and lightweight CQRS style

Decision:
- Build the backend as a modular monolith organized by feature slices, with clear boundaries between domain, commands and queries, HTTP endpoints, and infrastructure, without introducing a mediator library.

Rationale:
- This is the best fit for an MVP that still needs room to grow.
- It keeps deployment simple for Raspberry Pi while allowing the codebase to scale better than a flat web project.
- It supports a CQRS-like organization for reads and writes without paying the complexity cost of third-party mediator dependencies for a small system.

Alternatives considered:
- Single flat MVC project with no slice boundaries: simpler at first, but likely to become hard to evolve.
- Full mediator-based CQRS stack: familiar patterning, but unnecessary dependency and indirection for this scope.

Implications:
- Backend modules can expose minimal API endpoints mapped by feature.
- Commands and queries can stay explicit in code without a mediator abstraction layer.
- Tests can target behavior through HTTP and integration boundaries instead of implementation seams.

### 3. Use minimal APIs on the backend and Blazor Web App on the frontend

Decision:
- Prefer minimal APIs for backend endpoints and Blazor Web App for the frontend instead of MVC or Razor Pages, with Interactive Server as the default render mode in MVP.

Rationale:
- Minimal APIs keep the backend small and focused on feature behavior.
- Blazor components are a good fit for planner editing, shopping list interactions, and responsive UI state.
- Interactive Server keeps the initial architecture straightforward, avoids the extra client-project complexity of WebAssembly-first rendering, and is well suited to a small household app on a local network.
- Blazor Web App still keeps the door open for selective client-side rendering later if profiling or UX needs justify it.

Alternatives considered:
- Controller-heavy API design: valid, but more ceremony than needed for a feature-sliced modular monolith.
- Purely server-rendered frontend with no interactive component model: no longer aligned with the desired UX direction.

Implications:
- API contracts should be designed explicitly for frontend consumption.
- Frontend architecture should focus on route-driven views, component state, and mobile-first interaction patterns.
- Render-mode decisions should be conservative in MVP: use Interactive Server by default and only introduce client-side rendering when there is a clear benefit.

### 4. Use .NET Aspire for local orchestration

Decision:
- Use Aspire to run the Blazor Web App, backend, and PostgreSQL together in local development.

Rationale:
- This keeps local startup and environment configuration cohesive while the solution contains multiple moving parts.
- It reduces friction when running the app as a system rather than as isolated projects.
- It makes it easier to keep developer workflows close to the eventual deployed topology.

Alternatives considered:
- Manual multi-terminal startup scripts: workable, but less ergonomic and easier to drift.
- Docker-only local orchestration: useful later, but slower as the primary inner-loop workflow.

Implications:
- The solution should include Aspire AppHost and shared defaults for local execution.
- Connection strings, ports, and service wiring should be centralized for local development.

### 5. Use PostgreSQL as the default production database

Decision:
- Use PostgreSQL as the default deployment database, accessed via EF Core.

Rationale:
- Multiple devices on the local network may access the app concurrently.
- PostgreSQL gives better concurrency, migration discipline, and search headroom than an embedded file database.
- ARM-compatible Docker images and Linux hosting make PostgreSQL practical on Raspberry Pi-class environments.

Alternatives considered:
- SQLite: simpler footprint, but weaker concurrency and less comfortable as the app grows.
- MySQL/MariaDB: viable, but PostgreSQL tends to offer stronger ergonomics for rich querying and future search evolution.

Implications:
- The deployment baseline becomes at least two runtime components: app and database.
- Migrations, backups, and configuration need to be part of the platform story from the beginning.

### 6. Store uploaded images on disk and keep only paths in the database

Decision:
- Store uploaded recipe images in a dedicated folder on disk and persist only the relative or absolute file path reference in the database.

Rationale:
- This keeps the database smaller and avoids storing binary content in rows for a self-hosted Raspberry Pi deployment.
- It matches the current scope, where all images live in one locally managed directory.
- It keeps future storage abstractions possible without complicating the MVP.

Alternatives considered:
- Store image binaries directly in PostgreSQL: simpler referentially, but heavier operationally and less aligned with the desired filesystem approach.
- External object storage from day one: too much complexity for a LAN-hosted MVP.

Implications:
- The app must create and manage the image directory on startup or first write.
- File lifecycle rules must cover replacement and orphan cleanup.

### 7. Design the shell around library, details, and editor flows

Decision:
- The UI shell should revolve around three primary states: library, details, and editor.

Rationale:
- These states map directly to the highest-value use cases.
- They form a simple mental model that works especially well on phones.

Alternatives considered:
- Many top-level sections beyond recipes, meal plans, and shopping: increases surface area too early.
- Single-page everything editor: simpler technically, but harder to browse and maintain as the dataset grows.

Implications:
- Empty states, back-navigation, and unsaved-change handling are first-class UX concerns.
- The shell should prioritize recipes and planner as first-level destinations, with shopping reachable from the relevant plan flow.
- On larger screens, library and details can later coexist in a split layout without changing the underlying flow.

### 8. Make mobile-first responsiveness a hard requirement, not a later polish step

Decision:
- The application should be designed mobile-first from the first screen onward.

Rationale:
- The most probable day-one usage is opening the app on a phone while cooking.
- Retrofitting mobile usability later usually creates costly layout and interaction rework.

Alternatives considered:
- Desktop-first responsive adaptation later: likely faster in the first week, but creates UX debt immediately.

Implications:
- Navigation, forms, filters, and detail views must work within narrow viewports.
- Actions should stay thumb-reachable and avoid dense horizontal layouts.

## Target Architecture

```text
Phone / Tablet / Desktop Browser
              |
              v
        Blazor Web App
 (Interactive Server by default)
              |
              v
      Minimal API Backend
   (feature endpoints + contracts)
              |
   +----------+-----------+
   |                      |
   v                      v
Commands / Queries     Shared Contracts
   |
   v
Domain Model + Validation
   |
   v
Infrastructure (EF Core, PostgreSQL, file storage)

Local development:
Aspire AppHost
  -> Blazor Web App
  -> Minimal API backend
  -> PostgreSQL
```

Suggested feature slices:
- Backend:
- `Recipes/Library`
- `Recipes/Details`
- `Recipes/Editor`
- `Recipes/Search`
- `Planning/Plans`
- `Shopping/Lists`
- `Shared/Infrastructure`
- Frontend:
- `Recipes`
- `Planning`
- `Shopping`
- `Shared/Layout`

## Recommended Tech Stack

**Frontend**
- Blazor Web App
- Use Interactive Server as the default MVP render mode
- Keep client-side rendering as a future targeted optimization, not an initial requirement
- Mobile-first CSS, route-driven screens, and component-based UI composition

**Backend**
- ASP.NET Core in an LTS release
- Minimal APIs
- Feature-sliced handlers organized in command and query style without mediator libraries

**Data and Storage**
- EF Core
- PostgreSQL for the primary deployed database
- Structured migrations and seed support
- Local filesystem image storage with path references stored in the database

**Local Development**
- .NET Aspire AppHost and service defaults for running Blazor Web App, backend, and database together

**Hosting / Ops**
- Linux hosting on Raspberry Pi
- Docker Compose as the primary deployment path
- Native hosting as a secondary option if desired later
- Externalized configuration for connection strings, ports, file storage path, and data paths

**Testing**
- Focus on end-to-end tests that cover acceptance criteria and user stories
- Integration tests that verify externally visible behavior and contract-level correctness
- Include UI behavior coverage for key Blazor flows without coupling tests to component internals
- Avoid over-indexing on tests tied to implementation details or internal pattern structure

## Risks / Trade-offs

- [Risk] Blazor Web App plus separate API still adds more moving parts than a single server-rendered site. -> Mitigation: use Aspire for local orchestration and keep MVP on Interactive Server by default.
- [Risk] PostgreSQL adds operational overhead compared to SQLite. -> Mitigation: use Docker Compose, keep configuration minimal, and automate migrations and backup guidance.
- [Risk] Raspberry Pi resources are limited. -> Mitigation: prefer a modular monolith, keep dependencies light, and avoid unnecessary background services.
- [Risk] Free-text quantities make exact arithmetic aggregation difficult. -> Mitigation: keep units controlled, aggregate conservatively for MVP, and defer rich conversion logic.
- [Risk] Filesystem image storage can leave orphaned files when records change. -> Mitigation: define image replacement and cleanup rules early.
- [Risk] The current MVP scope may still be too broad for a first implementation slice. -> Mitigation: start implementation from recipe catalog first, then add planning, then shopping list behaviors.

## Migration Plan

No runtime migration is required because no application code exists yet.

Implementation sequence should be:
1. Scaffold the Blazor Web App frontend, minimal API backend, and Aspire AppHost for local orchestration.
2. Introduce the recipe domain model, filesystem image storage approach, and EF Core persistence with PostgreSQL.
3. Implement library, search, details, and editor use cases across frontend and backend feature slices.
4. Implement meal plan creation, recipe assignment, ordering, and multiplier handling.
5. Implement shopping list generation, checklist state, manual items, and copy action.
6. Add integration and end-to-end coverage focused on stories and acceptance criteria.
7. Add Linux-friendly deployment assets for Docker-based Raspberry Pi hosting.

## Open Questions

- Should plan and shopping flows live on separate top-level screens or should shopping remain nested under a selected meal plan in the initial UI?
- How much fuzzy title matching beyond case-insensitive and diacritic-insensitive search is worth adding in MVP?
