# Reset editor state when the same Blazor component changes recipe routes

## Why
The editor handles both /recipes/new and /recipes/{id}/edit, but the create route returns without resetting a previously loaded edit. Selected photos may also persist when switching between edited recipes on the same component instance.

## Changes
- On edit -> new navigation, reset the model, EditContext, errors, form status and photo state to a clean new recipe.
- On successful edit -> edit navigation, discard any pending photo from the previous recipe before applying the next draft.
- Disable the stale edit form when the requested recipe failed to load, to prevent saving previous content under another ID.
- Key InputFile by recipe route ID so browser file input is recreated between recipes.
- Add bUnit tests for both route transitions and for preventing cross-recipe photo upload.

## Non-goals
No new state store, backend/API contract, data migration or deployment change. Existing unsaved-change navigation confirmation stays intact.
