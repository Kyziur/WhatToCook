# OBS-05 application logging traceability

| Scenario | Verification |
| --- | --- |
| User title absent from successful, conflict and validation logs | `RecipeLoggingPrivacySpecificationTests.RecipeWriteLogs_ShouldNotContainUserTitlesOrNormalizedTitles` |
| Internal file paths not emitted by photo cleanup and upload logs | Source inspection of `EditorEndpoints.UploadMainPhoto`, existing recipe photo HTTP integration tests |
| Collector redaction, retention and metric cardinality | PENDING: homelab deployment and OTLP collector review |
