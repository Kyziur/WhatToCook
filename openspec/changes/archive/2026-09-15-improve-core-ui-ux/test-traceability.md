# Test Traceability

| Capability scenario | Automated evidence | Status |
|---|---|---|
| Initial shopping generation and atomic regeneration diff preserving matching MAM state and manual duplicates | `ShoppingListManagementSpecificationTests`; `shopping-list-management.spec.ts` | COVERED |
| Shopping loading, error, retry, and generation busy state | `ShoppingListManagementSpecificationTests` | COVERED |
| Core pages fit 375, 768, and 1440 CSS pixels | `recipe-app-shell.spec.ts` | COVERED |
| Search-first library, responsive disclosure, selected filters, and long-page navigation | `RecipeLibraryAndDetailsTests`; `recipe-app-shell.spec.ts`; `recipe-management-core.spec.ts` | COVERED |
| Ingredient names discard legacy quantity and unit prefixes | `RecipeLibraryAndDetailsTests`; `RecipeManagementCoreSpecificationTests` | COVERED |
| Multiple ingredient filters use all or any matching | `RecipeLibraryAndDetailsTests`; `recipe-management-core.spec.ts` | COVERED |
| Direct add to plan, duplicate prevention, and generated-list confirmation | `ResponsiveWorkflowUsabilitySpecificationTests`; `meal-planning.spec.ts` | COVERED |
| Compact plan selection, selected-day editing, busy state, and sticky actions | `MealPlanningSpecificationTests`; `meal-planning.spec.ts` | COVERED |
| Editor labels, Enter submission, focus, and validation relationships | `RecipeEditorSpecificationTests`; `RecipeImportSpecificationTests`; `recipe-management-core.spec.ts`; `recipe-imports.spec.ts` | COVERED |
| Large development profile provides 100 recipes and 20 plans | `LargeSeedSpecificationTests`; `agent-browser` visual review | COVERED |
| Remaining UI polish: unclipped direct-add panel, readable dates, mobile plan selector/menu behavior, four editor sections, and focusable planner actions | `dotnet build`; `agent-browser` desktop/mobile review | COVERED |
