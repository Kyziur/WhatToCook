# Recipe API ↔ Web consumer contract

The API and Web are independent deployables. DTOs remain **feature-local**; no global shared contracts assembly is introduced.

The recipe Web client treats the following as mandatory in JSON:
- Paged catalog: `items`, `totalCount`, `page`, `pageSize`, `tags`, `ingredients`, `tagUsage`.
- Summary: `id`, `title`, `servings`, `isActive`, `tags`, `ingredients`.
- Details: `id`, `title`, `servings`, `isActive`, `tags`, `ingredients`, `steps`. Each ingredient needs `ingredient`.

`source`, `mainPhotoPath`, `quantityText` and `unit` remain optional. `[JsonRequired]` rejects missing fields, **not** present-but-null values. The client also checks returned `page` and `pageSize` against the request, which matters when traversing all pages for the planner.

**Verification strategy:** negative Web HTTP-client tests catch missing fields or mismatched page metadata; an API integration test issues actual HTTP requests and validates the fields/types the Web consumes. Existing Playwright checks cover browser behavior. Extend these focused tests when changing another consumer boundary, rather than copying every feature DTO into a shared package or snapshotting the entire OpenAPI schema.

This does not establish compatibility of every endpoint or external consumer. Any independent deployment still requires coordinated API/Web releases if a contract changes incompatibly.
