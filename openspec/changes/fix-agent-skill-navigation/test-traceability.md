# AGENT-03 traceability

| Requirement | Verification | State |
| --- | --- | --- |
| All named `run-tests` skill references resolve locally | `node --test tests/ci/agent-skill-references.test.mjs` | PR CI pending |
| Existing instructions retain useful platform and filter detection without external missing skills | Static inspection of `.agents/skills/run-tests/SKILL.md` | Author-side review complete |
| Application behavior untouched | Diff limited to skill, agent CI test/workflow and OpenSpec/audit docs | Review pending |
