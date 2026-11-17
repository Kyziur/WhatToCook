---
name: task-delivery
description: Use when asked to fix something and open a PR, complete several requested outputs, or hand unfinished work to another person or agent. Track each requested result through verification and publication so a local patch is not mistaken for a finished task. Not for ordinary local edits or standalone document writing.
---

# Task delivery

## Outcome and scope

Deliver the requested result with fewer clarification loops, stale assumptions, and unfinished steps. This skill owns task coordination and completion, not code design or deployment safety. Follow the consuming repository's rules, authorization boundaries, and explicit user decisions. It grants no permission to publish, merge, or deploy.

Use only the steps the task needs. A small change does not need a written plan, status table, or new tracking document. Keep working notes in the existing task mechanism rather than creating a second source of truth.

## Decide what counts as done

Identify the requested result, exclusions, how to check success, and whether the user asked for a local change, a commit, publication, merge, or deployment. These are separate outcomes: finishing a patch does not finish a request to open a PR.

Resolve facts from current files, issue/PR state, and available history before asking. History explains intent, not current implementation or runtime state. Ask only for materially different outcomes or consequential decisions the evidence cannot settle. For reversible details, choose the repository's convention and proceed; state an assumption only when it affects the result.

Track each requested output separately so a successful first step does not hide unfinished work. A requested analysis or proposal is itself a complete deliverable; implementing it would change the user's intended scope.

## Establish current state

Before acting on a repository, identify its root, relevant checkout/revision, existing changes, and the source or target of the work. For PR tasks, inspect the current head, base, checks, and discussion as relevant. Check whether the requested work already exists before recreating it.

Refresh only evidence needed for the next decision. Preserve existing work and follow local checkout and temporary-file rules; a branch switch or cleanup can disrupt another person's changes even when the intended patch is correct.

## Do the work and handle blockers

Use the shortest sequence that completes the request. If delegation is available and useful, assign independent parts with a result, files, constraints, and check to run. One person or agent must combine shared changes, because independently correct patches can conflict when put together.

Continue until each requested output is finished or has a specific blocker. Complete independent work while blocked. Escalate with the missing decision or access, what it prevents, and the smallest action needed to proceed; another attempt without changed evidence will not resolve the same obstacle.

## Verify and close the loop

Exercise the changed behavior, not just the build; a passing build can leave an export broken, and local tests cannot establish production readiness. If installed, `pragmatic-senior` adds implementation and verification guidance, and `operational-change` adds stateful-transition guidance.

Before reporting completion, account for every requested deliverable. Report only the states that matter: implemented, verified, committed, published, merged, or deployed. Name the repository and link the relevant PR/commit when available. Do not list every possible state for a local edit.

When blocked on another person, or when the user requests a transfer, hand over the current result, remaining work, relevant evidence, and one concrete next step. Request only the minimum safe missing information, not secrets or facts already supplied. This skill decides when and what to transfer; use `documentation-quality` for how to write a usable issue or handoff if installed.

## Calibration

- **Application:** “Fix the export and open a PR” ends with an exercised export path and a PR link, not just a patch. Existing failure output is evidence; ask for more only if it cannot distinguish the remaining causes. Merge is outside this request.
- **Infrastructure:** “Prepare the deployment” requires establishing whether the result is a reviewed runbook, a tested deployment change, or authorized execution. Inspect the task context first; ask only if that consequential boundary remains unresolved. Report isolated rehearsal separately from live rollout.
- **Partial completion:** “Fix the export, update its guide, and open a PR” is not done when the export test passes. Finish the guide and publication, or identify which remaining result is blocked and why.
