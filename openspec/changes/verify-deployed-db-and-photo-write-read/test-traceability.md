# Test traceability

| Requirement | Automated check |
| --- | --- |
| Recipe create/read against actual deployed PostgreSQL | `SelfHostedWebPlatformSpecificationTests.ContainerBasedDeployment_ShouldStartSuccessfully` |
| Image upload/read from temporary host mount | Same test, checks file and HTTP response bytes |
| Production does not publish API port | Production Compose unchanged and existing rendered Compose network/port contract test |
| Actual Pi UID 1654 mount and existing persistent data | Operator-only; no CI claim |
