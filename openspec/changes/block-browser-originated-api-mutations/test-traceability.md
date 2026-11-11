# Test traceability

| Requirement | Evidence |
| --- | --- |
| Origin-bearing mutation is rejected | `CrossOriginBrowserRequest_ShouldNotCreateRecipe` |
| Cross-site upload rejected without photo write | `CrossSiteFetchMetadata_ShouldBlockPhotoUpload` |
| Server-side API call without browser headers works | `ServerToServerRecipeMutationWithoutBrowserHeaders_RemainsAllowed` |
| Protection against non-browser client, absent browser metadata, or direct API access | Not provided — use authorization and network isolation |
