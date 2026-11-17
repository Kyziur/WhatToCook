---
name: operational-change
description: Use before moving a stateful service, migrating stored data, cleaning up after a cutover, or deploying a change that old and new instances must survive together. Plan partial-failure recovery; execute only when authorized. Exclude routine stateless redeploys with no data or contract change, package/mobile releases, and documentation-only edits.
---

# Operational change

## Outcome and scope

Reach the intended state without losing data, hiding partial failure, or making recovery depend on memory. Use the existing deployment and backup tools. This skill supplies transition reasoning, not project-specific commands, an orchestration framework, or permission to operate a live environment.

Follow the consuming repository's approval, security, backup, and retention requirements. Keep the plan in the existing runbook, issue, or task notes; do not require a new document for every change. Apply detail in proportion to the actual risk.

Routine stateless redeploys with no data or contract change use the project's normal deployment procedure. Use this skill only if a concrete transition risk warrants it, not merely because a service restarts.

## Establish the transition

Identify current and target versions, consumers, state locations, ownership, and relevant writers. Configuration describes intended state; obtain permitted runtime evidence where actual mounts, versions, or ownership affect safety. Record unknowns that change the decision rather than guessing.

Determine what must remain true throughout the change: data ownership, a compatible reader/writer set, network isolation, or the ability to resume. Identify the existing tool that should enforce each dependency. Verify its semantics: deployment order is not readiness, and a successful API request is not necessarily a successful asynchronous operation.

For data at risk, establish backup scope, consistency, and recoverability before mutation. An existing backup is useful evidence, not automatic proof that the affected data can be restored. Reuse adequate existing evidence; do not demand another backup or restore exercise without a concrete gap or a local requirement.

## Plan partial failure, not just success

For each consequential transition, know:

- the precondition and observation that permit it;
- the state changed and who can still read or write it;
- the success signal and conditions requiring a stop;
- what remains if interrupted, and how to inspect that state before resuming.

Do not expand this into a ceremony for every command. Focus on boundaries where ownership, compatibility, data, or availability changes. If an aggregate operation partly succeeds, preserve its results and markers until inspected. Do not blindly rerun, delete state, or claim retry safety without evidence.

Use a real rollback only where the previous version can still safely consume the resulting state. For an explicitly chosen one-way cutover, plan recovery or forward completion instead of maintaining an unwanted legacy path. State the point of no return and any accepted downtime or data-loss window; obtain the required decision when these are not already agreed.

## Rehearse at the useful boundary

Use an isolated, authorized environment to exercise the risky transition and recovery/resumption path where feasible. Choose a representative failure or interruption at the boundary most likely to leave unsafe partial state, not a matrix of imagined failures. Use synthetic or explicitly permitted protected data; never copy production data or secrets into scratch space by default.

Check the invocation and permissions used by the real operator. Separate what rehearsal proves from host-specific or live checks still required. Missing live evidence is a gate for the affected operation, not a reason to invent success or to block unrelated preparation.

## Execute and verify when authorized

Confirm the target environment and required approval before mutation. Stop on failed preconditions or unexpected state; inspect before choosing recovery. Do not extend a preparation or review request into execution.

Verify final operation results, application behavior, ownership, and protection of new writes as relevant. A running process or green configuration parse is insufficient. Perform destructive cleanup only after the required acceptance and recovery evidence, retention constraints, and authorization are satisfied. Remove obsolete execution paths when the coordinated cutover and task scope permit it; do not keep them as unrequested fallbacks.

Report observed state, checks actually run, remaining gates, and the next permitted action. Never infer live completion from repository changes or isolated tests.

## Calibration

- **Application:** A schema change can leave old and new instances writing concurrently. Establish their compatibility and release order; exercise the transition with representative stored records. After incompatible writes, redeploying the old binary may not be a rollback.
- **Infrastructure:** Moving a stateful service between deployment owners requires identifying the actual storage and all writers, stopping conflicting writes, and verifying the new owner and application behavior. Before deleting old resources, confirm new writes are protected and the accepted recovery path remains available.
