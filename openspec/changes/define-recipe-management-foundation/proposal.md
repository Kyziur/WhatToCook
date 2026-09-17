## Why

Repository is currently a clean application template with OpenSpec enabled, but it does not yet define what product is being built. Before implementation starts, we need a shared product foundation for a recipe management application so that scope, architecture, and UI decisions are aligned from the first slice.

## What Changes

- Define the MVP scope for a household recipe management application shared by domownicy in a local network.
- Describe the core product capabilities needed in the first iteration: recipe catalog management, meal planning, shopping list generation, application shell/navigation, and self-hosted local-network access.
- Establish an initial web architecture direction that fits the repository stack, Raspberry Pi hosting, and future implementation without over-engineering.
- Capture a domain model, use cases, and UI direction for key flows and screens so future work can implement a coherent experience instead of isolated pages.
- Record non-goals and deferred areas to keep the first implementation slice focused.

## Capabilities

### New Capabilities
- `recipe-management-core`: create, view, edit, archive, organize, and search recipes with ingredients, steps, photo, source, and tags.
- `meal-planning`: create named date-range plans, assign recipes to days, and manage serving multipliers and ordering.
- `shopping-list-management`: generate plan-based shopping lists, track household item state, and copy lists as text.
- `recipe-app-shell`: provide the primary navigation model, screen structure, and state transitions for the recipe catalog, planner, and shopping experience.
- `self-hosted-web-platform`: provide browser-based access, relational persistence, and Linux-friendly self-hosted deployment for the application.

### Modified Capabilities
- None.

## Impact

- Affects future solution structure under `src/` and `tests/`, because this will be the first product feature set defined in the repository.
- Will drive later decisions around Blazor Web App plus API solution structure, relational persistence, responsive UI composition, filesystem-based image handling, Aspire-based local orchestration, and testing strategy.
- Will shape deployment conventions for Raspberry Pi, Linux hosting, and Docker-friendly local-network operation.
- Creates the baseline contract for implementation-ready OpenSpec artifacts in `openspec/changes/define-recipe-management-foundation/`.
