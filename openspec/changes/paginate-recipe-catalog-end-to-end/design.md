## API compatibility

Keep GET /api/recipes as a legacy list for existing consumers. The Blazor library switches to GET /api/recipes/page, returning {items,totalCount,page,pageSize,tags,ingredients,tagUsage}. Stable order uses explicit Id as final tiebreaker. Existing GET /api/recipes/search retains its list response but accepts page/pageSize and caps responses at 50. The search API preserves ANY-tag semantics, while library selections require ALL tags.

Filtering, counting and ID paging happen in PostgreSQL. Fetch full related entities only for IDs on the selected page. Whole-catalog facets are separately queried so options beyond page 1 remain available. The Web client retains filter state and offers next/previous pages; no client-side all-recipes preload.
