# Test traceability

| Requirement | Evidence |
| --- | --- |
| Writable storage is ready; no probe files remain | `RecipeImageStorageHealthSpecificationTests.WritableDirectory_ShouldBeHealthyAndLeaveNoProbeFile` |
| Missing storage fails without creation | `MissingDirectory_ShouldBeUnhealthyAndNotCreateIt` |
| Invalid storage path fails | `FileInsteadOfDirectory_ShouldBeUnhealthy` |
| Cancellation remains cancellation | `CancelledProbe_ShouldHonorCancellation` |
| Ready only, not live | `Program.cs` registers storage check with tag `ready`; `ServiceDefaults.Extensions.MapDefaultEndpoints` maps `/alive` from `live` only |
| Actual host permission and API upload/read | Operator evidence pending (not provable by CI) |
