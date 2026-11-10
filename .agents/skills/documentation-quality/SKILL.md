---
name: documentation-quality
description: Create, review, simplify, and maintain useful project documentation. Use when writing or restructuring README.md, docs navigation, developer guides, how-tos, runbooks, reference pages, architecture explanations, ADRs, or Mermaid/C4 diagrams; also use when code, infrastructure, or product changes make documentation stale.
---

# Documentation quality

## Outcome

Make documentation **useful, findable, correct, and easy to understand**. A reader should be able to answer a question, accomplish a task, or understand a decision without hunting through unrelated files. The best change may be to simplify, link, or delete existing material rather than add another page.

This skill applies to software applications, libraries, infrastructure, and operational projects. Follow the consuming repository's `AGENTS.md`, source-of-truth conventions, language, security rules, and deployment permissions. This skill is a writing and review standard, **not** permission to operate live systems.

## Start with the reader

Before changing docs, identify:

1. **Audience:** end user, developer, new contributor, maintainer, operator, or reviewer.
2. **Question/job:** what do they need to know or do?
3. **Entry point:** how will they find the answer, starting from `README.md`?
4. **Evidence:** which current code, configuration, specification, decision, or dated observation supports the information?
5. **Smallest useful edit:** update one existing page, improve navigation, add a focused page, or make no change.

Do not generate documentation because a file, feature, or diagram exists. A document needs a real reader purpose and a discoverable home.

## README: the front door

A root `README.md` should let a new reader understand the repository quickly, in this order:

1. **What is this?** Explain the product/project in plain language, before naming technologies.
2. **Why does it exist, and who uses it?** Name the problem and intended audience.
3. **What can it do?** Mention only the main capabilities or responsibilities.
4. **How do I begin?** Give one safe first action, or point to the appropriate guide.
5. **Where next?** Link to the most useful journeys: use, develop, deploy/operate, architecture, contribution as relevant.
6. **Critical caveats:** Mention only restrictions that change an initial decision (for example, no public exposure, unsupported platform, or external deployment ownership).

Make the README a concise orientation page, **not** a command dump, change log, mirror of the docs tree, or encyclopedia. Keep the why/what/for whom near the top. Code-focused libraries may lead to a short usage example; infrastructure repositories may link to a reviewed deployment runbook instead of exposing destructive commands on the front page.

Add `docs/index.md` only when navigation from README becomes difficult. Organize an index by reader goals (for example, “I want to deploy a change”) instead of listing every Markdown file. Every important page needs a sensible navigation path from README, directly or through an index.

## Organize by reader need, not a fixed folder template

Apply [Diátaxis](https://diataxis.fr/) as a **writing model**, not as mandatory directories:

| Reader question | Form | Write |
| --- | --- | --- |
| “Can you teach me?” | Tutorial | A guided, safe learning path with checkpoints |
| “How do I do X?” | How-to / runbook | Preconditions, ordered actions, verification |
| “What is X exactly?” | Reference | Precise APIs, options, commands, schemas and contracts |
| “Why is X this way?” | Explanation | Mental model, architecture, constraints, trade-offs |

Use additional documents when there is a clear purpose:

- **ADR:** durable, significant decision; context, alternatives, choice, consequences, status and date.
- **Requirements/specification:** intended behavior and acceptance criteria; link to the consuming repo's canonical spec system rather than copying it.
- **Audit/incident/evidence:** dated findings; archive or label clearly and never present as current operational truth.

Start small: `README.md` plus focused pages under `docs/` as needed. Preserve useful conventions in existing repositories. Do not create empty `tutorials/`, `how-to/`, `reference/`, or `architecture/` folders just to resemble a framework. Split mixed-purpose pages only if doing so improves clarity; related pages should link rather than repeat each other.

Prefer task- or subject-oriented filenames that remain meaningful after implementation details change. Keep relative links stable; update incoming links when moving or renaming pages.

## Write instructions people can actually follow

For a task guide or tutorial:

- Lead with the **result** and the audience; give prerequisites before the first action.
- Provide the shortest safe ordered path. State the working directory, platform/shell and access level when they matter.
- Give copyable commands and explain what each non-obvious command does, including its effects and where it runs.
- Explain required parameters, expected output or success signals, and common failure/STOP conditions.
- Put conceptual background and alternatives after the critical path or link to them.
- Mark examples as examples; do not pretend sample output was observed on the reader's environment.

For an **operational runbook**, additionally establish before mutations:

- What is read-only, what changes state, and what is destructive.
- Authorization, backup/recovery prerequisites, dependency health and any maintenance window.
- Explicit **STOP** conditions ahead of dangerous steps.
- Verification beyond “process started,” and a truthful rollback or recovery path.
- What is **NOT VERIFIED** or requires a live operator check.

Never invent a rollback, promise safety, or run production/destructive steps merely to test prose. A runbook with an unproven recovery path is not production-ready.

For **reference**, state exact names, types, defaults and constraints; prefer generated contracts when reliable. For **architecture**, explain boundaries, responsibilities, relationships, primary data flows, and important trade-offs. For **ADRs**, record *why* a decision was made, not a second copy of a configuration file.

## Diagrams: choose only when they clarify

A diagram must answer a question more effectively than a paragraph. Select the smallest useful form:

| Question | Good choice |
| --- | --- |
| Who/what connects to this system? | C4 system context |
| Which services, applications or stores exist? | C4 container-level view |
| In what order do participants interact? | Sequence diagram |
| What decision path or operation should I follow? | Flowchart |
| Which states and transitions matter? | State diagram |
| Where does software run, and across which boundaries? | Deployment/topology diagram |

C4 describes **abstraction levels**, not a requirement to use C4-specific diagram syntax. Prefer editable Mermaid in Markdown if supported; a plain Mermaid flowchart can express a C4-like view. Reuse the repository's established diagram tools otherwise. Do **not** default to component- or code-level C4 views.

Every included diagram should:

- Have a title/lead-in stating the question it answers.
- Name nodes by role and arrows by meaning; show direction.
- Respect the chosen abstraction level; omit irrelevant detail.
- Make relevant external, security and trust boundaries understandable.
- Distinguish **implemented/current**, **planned**, and **historical** states.
- Include a short text explanation so the page is useful without rendering.
- Render in the target Markdown environment where verification tools are available.

Never invent system edges, topology, trust assumptions, or deployment state. Prefer one legible diagram over a giant all-services chart. Avoid decorative diagrams, oversized edge labels, screenshots of editable diagrams, and unverified generated visuals.

## Style: optimize for comprehension

Follow the intent-first principles of [Microsoft Learn](https://learn.microsoft.com/en-us/contribute/content/style-quick-start) and the terminology discipline of the [Google developer style guide](https://developers.google.com/style/):

- Answer **what/why/how** early; use concise, descriptive headings and concrete verbs.
- Use direct, plain language without reducing technical precision.
- Explain unfamiliar abbreviations once; use terms consistently.
- Keep paragraphs and steps short enough to scan. Do not interrupt an ordered procedure with digressions.
- Show a realistic example when it removes ambiguity, not as decoration.
- Prefer one recommended happy path; put edge cases where relevant.
- Use tables for genuine comparisons, not layout. Use bullets only when they aid scanning.
- Avoid boilerplate, marketing claims, filler (“obviously”, “simply”), vague promises and redundant screenshots.
- Follow the project's documentation language. Do not silently translate an established repo.
- Do not impose arbitrary word limits. Remove words that add no reader value.

## Truth, scope, and maintenance

Check each important claim against its **appropriate** source:

| Source | What it proves / does not prove |
| --- | --- |
| Code and tests | Implemented behavior or test expectations; tests may not cover everything |
| Compose, IaC, manifests, scripts, schemas | Desired/configured state, **not** live deployment |
| Product specs / OpenSpec | Intended contract, **not** confirmed implementation |
| Runbooks | How to perform an operation, **not** that it happened |
| ADRs | The decision and its reasoning at the time |
| Dated telemetry/operator evidence | Observed runtime facts only for that date and scope |

Mark `planned`, `implemented`, `configured`, `deprecated`, or `NOT VERIFIED` when a distinction matters. Never invent deployment evidence, external URLs, API behavior, commands, security properties, or test results.

Update documentation **in the same change** when behavior or operating procedures change. Prefer editing the canonical page to adding another one. Link to source configuration and specifications instead of duplicating lists of settings, volumes, resources or API contracts that will drift. Archive valuable historical evidence with dates, and remove obsolete active instructions. Do not rewrite unrelated docs for stylistic uniformity.

Protect secrets in examples, terminal output, diagrams and troubleshooting instructions. Follow the local project's permission boundaries before making any runtime observations.

## Author/review workflow

1. **Discover:** inspect root README, local rules, docs navigation, changed source/config/specs, and affected pages.
2. **Decide:** state the reader question, canonical destination and minimal change required. Avoid new pages when an existing one will do.
3. **Edit:** make the smallest coherent update; put the answer first and link to deeper detail.
4. **Verify correctness:** compare paths, commands, options, examples, architecture and safety claims against current sources.
5. **Verify discoverability:** follow links from README; check moved pages, relative paths and relevant anchors.
6. **Verify visuals:** check Mermaid syntax/rendering when tooling is available and diagram semantics against actual source.
7. **Review as the reader:** can the intended reader find, understand, apply and (if necessary) safely reverse the information?
8. **Report evidence:** say what changed, which checks actually ran, what could not be checked, and any operator-only gates.

### Acceptance checklist (proportionate to the change)

- **Purpose:** every page answers a distinct question.
- **Navigation:** relevant readers can find it from README/index.
- **Accuracy:** factual and current/planned claims are supported.
- **Actionability:** instructions explain prerequisites, commands and success conditions.
- **Safety:** dangerous actions have prerequisites, STOP conditions and honest recovery notes.
- **Clarity:** a competent newcomer understands without internal tribal knowledge.
- **Focus:** no unnecessary page, diagram, duplication or unrelated rewrite.
- **Evidence:** validation results are real; missing checks are marked `NOT VERIFIED`.

Examples of good restraint: an internal refactor with no observable consequences may need **no documentation change**; a configuration change may need **one runbook update**; a new app may need only **README + one getting-started guide**. Documentation quality is not measured by page count.
