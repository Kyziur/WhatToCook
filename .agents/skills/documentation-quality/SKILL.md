---
name: documentation-quality
description: Write, review, simplify, and maintain README, guides, runbooks, references, architecture, ADRs, and diagrams. Also use when code, infrastructure, or product changes make documentation stale. Covers clear issue and handoff wording, not task coordination or deciding when to transfer work.
---

# Documentation quality

## Outcome

Make documentation **useful, findable, correct, and easy to understand**: a reader should answer a question, finish a task, or understand a decision without hunting. Often the best change simplifies, links, or deletes material instead of adding a page.

This applies to applications, libraries, infrastructure, and operational projects. The consuming repository's agent instructions (e.g. `AGENTS.md`), source-of-truth conventions, security rules, and deployment permissions take precedence. This is a writing and review standard, **not** permission to operate live systems.

## Start with the reader

Before editing, identify the audience, the question or job they have, the entry point (`README.md`, or the issue, PR, or thread), the evidence supporting the content, and the smallest useful edit. Do not document something merely because a file, feature, or diagram exists.

## README and navigation

The root README is a front door: what the project is in plain language, why it exists and for whom, its main capabilities, one safe first step or a pointer to the right guide, where to go next, and only the caveats that change an early decision. It is not a command dump, changelog, or docs-tree mirror. Infrastructure READMEs link to a reviewed runbook rather than exposing destructive commands.

Add `docs/index.md` only when README navigation becomes hard; organize it by reader goal, not by file list. Every important page must be reachable from the README, directly or through an index.

## Organize by reader need

Use [Diátaxis](https://diataxis.fr/) as a writing model, not as mandatory folders: tutorials teach, how-tos solve a task, references state exact facts, and explanations give reasoning. Add other documents only for a clear purpose:

- **ADR:** a durable, significant decision: context, alternatives, choice, consequences, status, and date.
- **Requirements:** intended behavior and acceptance criteria. Link to the repository's canonical spec system rather than copying it.
- **Audit or incident record:** dated findings, labeled, never presented as current truth.

Start small and follow existing repository conventions. Do not create empty `tutorials/`, `how-to/`, `reference/`, or `architecture/` folders. Prefer subject-oriented filenames, and update incoming links when moving pages.

## Instructions people can follow

- Lead with the result and prerequisites. Give the shortest safe ordered path, stating working directory, platform, and access level when they matter.
- Explain each non-obvious command, its effects, and its success signal; name likely failures and the next action.
- Explain each required value or link to its authoritative source or secure acquisition method; readers should not have to guess where a token or password comes from.
- Mark examples as examples; never present sample output as observed.
- Repeated manual commands, unchanged inputs, or remembered ordering can signal missing automation. Flag the implementation gap instead of elaborating the procedure, and implement automation only within the authorized scope.
- For reference pages, state exact names, types, defaults, and constraints; prefer generated contracts when reliable. For architecture, explain boundaries, responsibilities, data flows, and important trade-offs.

Operational runbooks additionally establish before mutations:

- Read-only, state-changing, and destructive steps; authorization and maintenance windows.
- Backup/recovery prerequisites, dependency health, and explicit **STOP** conditions.
- Application-level success signals, a truthful recovery or rollback path, and what remains **NOT VERIFIED** or needs a live operator check.

Never run production or destructive steps to test prose or invent a rollback. An unproven recovery path is **NOT VERIFIED**, not production-ready. For migrations, consult `operational-change` if installed.

## Specialized references

- Adding or reviewing a diagram: read [Diagrams](references/diagrams.md) before choosing its form.
- Substantive prose authoring or review: read [Style](references/style.md). Skip it for routine typo or link fixes.

## Artifact language

Write repository artifacts (documentation, code comments, issues, PRs, reviews, and PR/issue comments) in the project's established language, even when the conversation uses another. Do not silently translate an established repository.

## Truth and evidence

Check important claims against the appropriate source:

| Source | What it proves / does not prove |
| --- | --- |
| Code and tests | Implemented behavior or test expectations; tests may not cover everything |
| Compose, IaC, manifests, scripts, schemas | Desired/configured state, **not** live deployment |
| Product specs | Intended contract, **not** confirmed implementation |
| Runbooks | How to perform an operation, **not** that it happened |
| ADRs | The decision and its reasoning at the time |
| Dated telemetry/operator evidence | Observed runtime facts only for that date and scope |

Mark `planned`, `implemented`, `configured`, `deprecated`, or `NOT VERIFIED` when the distinction matters. Never invent deployment evidence, external URLs, API behavior, commands, security properties, or test results. Protect secrets in examples, output, diagrams, and troubleshooting. Follow local permission boundaries before any runtime observation.

## Maintenance

- Update documentation **in the same change** when behavior or operating procedures change. Edit the canonical page rather than adding another.
- Link to source configuration and specifications instead of copying settings, volumes, resources, or contracts that will drift.
- Keep a historical document only for a named purpose (an operational record, a legal or retention obligation, or a decision trail). State that purpose and any retention constraint where readers will see it.
- When the requested documentation cleanup covers an obsolete page, remove it rather than archiving it unless local approval or retention rules restrict deletion. No separate approval is needed solely because the in-scope edit deletes a page. Preserve unrelated work; if deletion is restricted, label the page historical or superseded and link to its replacement.
- Do not rewrite unrelated docs for stylistic uniformity.

## Review workflow

1. **Discover and decide:** read the README, local rules, changed sources, and affected pages (for an issue or handoff, its thread and PR). State the reader question, the canonical destination, and the minimal change.
2. **Edit:** answer first, link to detail, and keep the update the smallest coherent change.
3. **Verify:** check claims, paths, commands, and examples against current sources; check links, anchors, and each step's success signal; confirm no real secret appears. Walk procedures by reading or by a dry run the project explicitly allows; never use live or destructive steps as prose tests.
4. **Review as the reader:** can they find, apply, and, if necessary, safely reverse the information?
5. **Report:** what changed, which checks actually ran, what remains `NOT VERIFIED`, and any operator-only gates.

Good restraint: an internal refactor with no observable effect may need **no documentation change**; a configuration change may need **one runbook update**; a new app may need only **README + one getting-started guide**. Documentation quality is not measured by page count.

## Issues and handoffs

A useful issue or handoff lets someone act without rereading the thread: entry point and status (including what remains NOT VERIFIED), the observable result wanted, scope and non-goals, dependencies and order, constraints (permissions, live access, data safety, language), and acceptance checks with who may run each. Follow the repository's own issue and PR conventions. The `task-delivery` skill, if installed, decides when work transfers and what a handoff carries.
