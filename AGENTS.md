# Agent Set

This repository uses a small, practical subagent set.

Default rule: do not spawn subagents unless the task benefits from delegation. Most changes should be completed by one main implementation agent end-to-end.

## Core Roles

### `discovery-spec`

- Type: `explorer`
- Purpose: clarify scope, inspect the codebase, and shape or update OpenSpec artifacts before implementation.
- Owns: repository exploration, change framing, proposal/design/tasks refinement, risk discovery.
- Does not own: feature implementation, broad refactors, writing production code.
- Typical use:
  - the request is ambiguous,
  - architecture tradeoffs need to be compared,
  - an OpenSpec change should be proposed or tightened before coding.
- Expected output:
  - short problem summary,
  - relevant codebase findings,
  - recommended scope,
  - suggested OpenSpec updates.

### `implementation`

- Type: `worker`
- Purpose: default delivery agent for product work.
- Owns: application code, refactors, bug fixes, unit tests, integration tests, and local verification for the change it implements.
- Does not own: independent review of its own work after completion; broad repo/platform maintenance unless the task is specifically infrastructure-oriented.
- Typical use:
  - API endpoint or Minimal API work,
  - MAUI feature work,
  - frontend integration,
  - bug fixes,
  - adding or updating tests.
- Expected output:
  - focused code changes,
  - matching tests,
  - verification notes,
  - any OpenSpec task checkbox updates if applicable.

### `quality-review`

- Type: `explorer`
- Purpose: provide an independent second pass on correctness and maintainability.
- Owns: code review, regression risk analysis, test gap detection, coverage and CRAP-oriented feedback, convention checks.
- Does not own: being the default author of the feature or the default author of tests.
- Typical use:
  - after a substantial implementation,
  - before merging high-risk changes,
  - when confidence is lower than usual,
  - when the change touched auth, persistence, or cross-cutting behavior.
- Expected output:
  - prioritized findings,
  - missing test scenarios,
  - regression risks,
  - follow-up recommendations.

### `repo-platform`

- Type: `worker`
- Purpose: maintain the template itself and other cross-cutting repository mechanics.
- Owns: CI, build scripts, `Directory.Build.*`, `Directory.Packages.props`, Husky, formatting, bootstrap scripts, template conventions, shared developer tooling.
- Does not own: normal feature delivery unless the feature is primarily repo/platform work.
- Typical use:
  - updating the template,
  - adding new skills,
  - adjusting OpenSpec workflow scaffolding,
  - improving build and test automation.
- Expected output:
  - focused infrastructure changes,
  - updated repo conventions,
  - verification notes for tooling or pipeline behavior.

## Working Agreement

### Default Flow

1. Use `implementation` alone for most feature and bug-fix work.
2. Add `quality-review` when the change is substantial or risky.
3. Use `discovery-spec` before coding only when scope is unclear or OpenSpec work matters.
4. Use `repo-platform` only for template, tooling, or repository-wide changes.
5. If more than one OpenSpec change is `in-progress`, explicitly name the active implementation target before editing code.

### Active Change Hygiene

- Prefer one active implementation change at a time.
- If a second `in-progress` change is only backlog or design work, keep treating it as design-only and say so explicitly in chat, tasks, or handoff notes.
- When implementation starts, mention the active change name early and keep spec, tests, and verification aligned to that change.

### Tool and Shell Compatibility

- Treat this repository as PowerShell-first. When a skill, prompt, or OpenSpec artifact shows bash-style commands, translate them to equivalent PowerShell commands instead of following the shell syntax literally.
- If a skill refers to `AskUserQuestion`, use `request_user_input` when available; otherwise ask the user a single concise plain-text question.
- If a skill refers to `TodoWrite`, use `update_plan`.
- If a skill assumes repo-local shell helpers that do not exist in this Codex environment, prefer `shell_command`, Browser plugin capabilities, and standard repository commands instead of inventing missing tools.
- Keep these compatibility rules in repo instructions instead of forking generated OpenSpec skill content whenever possible.

### Verification Workflow (Mandatory)

- After each package of code changes, run verification before moving on.
- Run verification commands **sequentially**. Do not run `dotnet build` and `dotnet test` in parallel in this repository (prevents `obj/bin` and static web assets file locks).
- Set an explicit shell timeout for non-interactive verification commands rather than relying on the 120-second default: `dotnet build` 5 minutes, `dotnet test` 10 minutes, scoped Playwright E2E 5 minutes, and full Playwright E2E 15 minutes. Start local servers through the tracked background-process tool with a readiness probe, never as a blocking shell command.
- Use two verification passes:
  - `inner-loop`: during implementation, validate UI changes through the `agent-browser` skill and run only the smallest relevant unit or component tests for logic that changed. Do not use Playwright E2E as a check after every small UI edit.
  - `handoff`: run the full required sequence once, immediately before the final handoff of a complete user-requested task, unless the user explicitly agrees to a blocker or narrower scope.
- A delivery package is the complete user-requested task, not an intermediate edit, a small fix discovered during the task, or a checkpoint before the next edit. Do not run the full Playwright suite between small changes in the same task.
- Minimum verification at final handoff of each delivery package:
  - `dotnet build WhatToCook.slnx`
  - `dotnet test WhatToCook.slnx` (or the relevant filtered scope when intentionally scoped)
  - `PLAYWRIGHT_HTML_OPEN=never npx playwright test --config=playwright.config.ts`
  - `dotnet husky run`
- If `dotnet husky run` skips because no files are staged, run `dotnet csharpier format .` before continuing.
- Preferred `inner-loop` examples:
  - planner-only or recipe-import-only UI changes: start the local application once, then use `agent-browser` to inspect the changed workflow at desktop and mobile viewport sizes, exercise its primary interaction, and check the browser console. Run targeted component tests only when component behavior changed.
  - backend-only contract or domain changes: run build, relevant `dotnet test` filters, then add E2E only if the changed contract or workflow is user-visible
  - spec/docs/skill/tooling-only changes: run the narrowest meaningful smoke check for the affected tool; full E2E is not required unless the task also changes application behavior
- Treat E2E as the final regression gate after the UI and implementation are stable, not as a substitute for fast browser-assisted iteration. Run a scoped E2E subset during implementation only when E2E tests or the browser flow changed, to diagnose an E2E-specific failure, or when the user explicitly requests it.
- If a non-interactive command times out, inspect its output and running processes before retrying. Increase the timeout only when the command is making expected progress; do not blindly repeat full E2E runs or stop processes not started by the current agent.
- If any command fails, treat the package as incomplete:
  - report the exact failure,
  - fix it or clearly document a blocker,
  - rerun the same verification commands.
- Do not continue to the next implementation package until the current package verification is green or an explicit blocker is agreed.

### Test Ownership

- Tests belong to `implementation` by default.
- `quality-review` checks whether test coverage is adequate and points out gaps.
- Split test writing into a separate worker only for large tasks with clearly separate file ownership.

### Spec-to-Test Traceability (Mandatory)

- Treat each business scenario in `openspec/changes/<active-change>/specs/**/*.md` as a testable contract.
- For every implemented scenario, add at least one automated test and keep tests grouped by capability/spec name where practical (for example `recipe-management-core`, `recipe-app-shell`, `meal-planning`).
- If a scenario is intentionally not implemented yet, record it in the test traceability matrix with `PENDING` and the owning OpenSpec task id.
- Integration tests SHOULD use Testcontainers-backed real infrastructure (for this repository: PostgreSQL) instead of SQLite test doubles, unless a task explicitly requires a different target.

### OpenSpec Design Backlog Flow (Mandatory)

- If a useful product idea is identified but intentionally not implemented in the current package, capture it as a **design-only** OpenSpec change instead of leaving it in chat notes.
- A design-only OpenSpec change MUST include:
  - `.openspec.yaml`
  - `proposal.md`
  - `tasks.md`
  - at least one capability spec file under `specs/**/spec.md`
- Such design-only entries MUST clearly state non-goals (`no implementation yet`) and include initial test-traceability placeholders as `PENDING`.

### Delegation Rules

- Prefer one active worker unless there is real parallelism.
- Do not split work by technology just for symmetry.
- Spawn parallel workers only when write ownership is clearly separated.
- If two workers are used, each must have an explicit file or module boundary.
- Never ask `quality-review` to quietly finish the implementation; if review reveals missing work, hand it back to `implementation`.

## Recommended Usage Patterns

### Small Change

- Use `implementation`.

### Ambiguous Feature

- Use `discovery-spec`, then `implementation`.

### High-Risk Change

- Use `implementation`, then `quality-review`.

### Template or Tooling Update

- Use `repo-platform`, then `quality-review` if the change affects build, CI, testing, or shared conventions.

### Large Cross-Stack Feature

- Start with `discovery-spec`.
- Use one `implementation` worker if the work is still manageable in a single lane.
- Split into multiple workers only when backend and client work can be isolated cleanly.

## Handoff Contract

When one agent hands work to another, include:

- the goal of the change,
- exact files or modules in scope,
- what was already verified,
- known risks or open questions,
- whether OpenSpec artifacts were updated,
- whether more tests are still needed.

## Repository Preference

This template is optimized for a low-overhead workflow:

- one main implementation worker most of the time,
- one optional reviewer for independent feedback,
- one optional discovery pass for architecture and OpenSpec,
- one optional platform worker for template and tooling changes.

<!-- REPOWISE_AGENTS:START — Do not edit below this line. Auto-generated by Repowise. -->
## Codebase Intelligence for WhatToCook (Repowise)

Indexed by [Repowise](https://repowise.dev). Last indexed: 2026-09-08 (commit 50d4d4d). Confidence: 100%.
### How to work in this repo

- **Trust the index.** `verified: true` and `_meta.complete` mean the bytes were checked against the live tree, so never re-read them. Re-read only what `bounds: "approximate"` or `_meta.stale_warning` names. `confidence` rates the prose, not the evidence: on `low` read the `fallback_targets` or `best_guesses` the reply names, and run `repowise update` and ask again if `_meta.hint` says the index is behind HEAD. `index_behind: true` alone is informational.
- **A zero carries its basis.** An empty `callers`/`callees`/`used_by` comes with a `*_basis` saying how much of that language's calls the graph resolved, so read it before concluding nothing calls a symbol. `_meta.scope_hint` names the areas the answer did not touch.
- **Pre-edit, not instead-of-edit.** These tools decide *which* files to read and edit. Reading a file before you edit it is correct and expected.
- **Noisy commands** (tests, builds, `git log`/`diff`, searches, listings): prefer `repowise distill <cmd>`, the same command with its exit code preserved and errors-first output. A `[repowise#<ref>: N lines omitted]` marker is recoverable via `repowise expand <ref>` (add `-q <regex>` to filter); never re-run the command to see omitted output.
- **Recording a decision** you had to reason out: `repowise decision add --title T --decision D` records it without prompting and prints the id (`--format json` to parse it back). It lands `proposed`, for a person to confirm.

### Tools

| Tool | When and why |
|------|--------------|
| `get_answer(question)` | First call for any how/where/why question. Cite `confidence: "high"` or `grounding: "extracted"` directly; `degraded` means judge by `retrieval_quality`. `symbol_bodies` has live bodies. |
| `get_context(targets=[...])` | Triage card for files/modules/symbols: docs, signatures, hotspot, fix history. No source bytes — `include=["skeleton"]` for the whole file verified, `["callers"|"decisions"]` for depth. Batch targets. |
| `get_symbol(id, depth?)` | **Follow-up, not an entry point** — one verified body for an id a prior response named (`path.py::Name`, `path.py:140-180`, `repowise#<hex>`). Never walk a file symbol by symbol; Read it. |
| `search_codebase(query)` | Hybrid search, auto-routed by query shape; force with `mode=symbol|path|concept|hybrid`. A hit whose `sources` are `[fts]` only has no semantic agreement, so verify it. |
| `get_why(query, targets?)` | Why the code is shaped this way: decision records, git archaeology, rationale comments. Call before a refactor or a pattern divergence. |
| `get_risk(targets, changed_files?, include?)` | File history and structural reach. PR mode leads with `directive`; its 0-10 structural heuristic is uncalibrated, not a probability. Read typed test recommendations and coverage state first. |
| `get_change_risk(revspec?, extensions?, exclude_patterns?)` | Deterministic live-diff review signal for a commit or range. Lead with benchmarked percentile/classification; the 0-10 diff-shape score is supporting, not a probability. `get_risk` scores paths. |
| `get_health(targets?, include?)` | Defect / maintainability / performance scores and findings. Self-check the files you touched before finishing. |
| `get_dead_code(tier?, min_confidence?, safe_only?)` | Confidence-tiered unreachable files / unused exports / zombie packages. For cleanup sweeps, not targeted fixes. |
| `get_overview()` | Architecture map. Call once, first, in an unfamiliar repo; skip it after that. |

### Architecture
**Files:** 248 | **Lines:** 33204 | **Import cycles:** 3
WhatToCook is a markdown codebase of 248 files. Execution starts at src/WhatToCook.Web/Components/App.razor, src/WhatToCook.Api/Program.cs, src/WhatToCook.Web/Program.cs. ---
*Built from the code's structure. It states what is there, not why it is that
way.

### Key modules
- `src/WhatToCook.Api/Domain` — src/WhatToCook.Api/Domain/Planning · src/WhatToCook.Api/Domain/Recipes · src/WhatToCook.Api/Domain/Shopping
**Language:** csharp |…
- `src/WhatToCook.Api/Domain/Planning` — src/WhatToCook.Api/Domain/Planning · src/WhatToCook.Api/Domain/Recipes
**Language:** csharp | **Files:** 13 | **Public symbols:** 117 /…
- `src/WhatToCook.Api/Features/Recipes` — src/WhatToCook.Api/Features/Recipes/Details · src/WhatToCook.Api/Features/Recipes/Editor · src/WhatToCook.Api/Features/Recipes/Imports ·…
- `src/WhatToCook.Api/Features/Recipes/Imports` — src/WhatToCook.Api/Features/Recipes/Imports
**Language:** csharp | **Files:** 15 | **Public symbols:** 120 / 218
Covers the 15 source files…
- `src/WhatToCook.Web/Components/Features/Recipes/Details/Pages` — src/WhatToCook.Web/Components/Features/Recipes/Details/Pages · src/WhatToCook.Web/Components/Features/Recipes/Editor/Pages ·…
- `src/WhatToCook.Api/Infrastructure/Data` — src/WhatToCook.Api/Infrastructure/Data · src/WhatToCook.Api/Infrastructure/Data/Migrations ·…
- `src/WhatToCook.Api/Domain/Shopping` — src/WhatToCook.Api/Domain/Shopping · src/WhatToCook.Api/Features/Planning/Plans · src/WhatToCook.Api/Features/Recipes/Details ·…
- `src/WhatToCook.Web` — src/WhatToCook.Web · src/WhatToCook.Web/Components · src/WhatToCook.Web/Components/Layout · src/WhatToCook.Web/Components/Pages ·…
- `src/WhatToCook.Web/Components/Features/Planning/Components` — src/WhatToCook.Web/Components/Features/Planning/Components · src/WhatToCook.Web/Components/Features/Planning/Pages ·…
- `src/WhatToCook.Api/Features/Recipes/Shared` — src/WhatToCook.Api/Features/Recipes/Shared
**Language:** csharp | **Files:** 5 | **Public symbols:** 58 / 75
Covers the 5 source files in…

### Entry points
- `src/WhatToCook.Web/Components/App.razor`
- `src/WhatToCook.Web/wwwroot/app.js`
- `src/WhatToCook.Api/Program.cs`
- `src/WhatToCook.Web/Program.cs`

### Files that need care (bug-fix history first, then churn — check `get_risk` before editing)
- `src/WhatToCook.Web/Components/Features/Planning/Pages/MealPlannerPage.razor` — 4 commits/90d
- `src/WhatToCook.Web/Components/Features/Planning/Pages/MealPlannerPage.razor.cs` — 4 commits/90d
- `src/WhatToCook.Web/Components/Features/Recipes/Library/Pages/RecipeLibraryPage.razor` — 3 commits/90d
- `src/WhatToCook.Web/Components/Features/Shopping/Pages/ShoppingListPage.razor` — 3 commits/90d
- `src/WhatToCook.Web/Components/Features/Recipes/Editor/Pages/RecipeEditorPage.razor` — 3 commits/90d

### Code health
Three co-equal signals: defect risk 8.59/10 avg, hotspot health 2.7/10 (declining), worst `src/WhatToCook.Web/Components/Features/Planning/Pages/MealPlannerPage.razor.cs` at 1.0/10 · maintainability 9.19/10 · performance risk 5 open static I/O-in-loop / N+1 findings. Detail: `get_health()`.

Critical files:
- `src/WhatToCook.Web/Components/Features/Recipes/Editor/Pages/RecipeEditorPage.razor.cs` — god class (RecipeEditorPage) — impact −2.3
- `tests/WhatToCook.Api.IntegrationTests/Specs/SelfHostedWebPlatformSpecificationTests.cs` — change entropy — impact −2.0
- `src/WhatToCook.Web/Components/Features/Shopping/Pages/ShoppingListPage.razor` — change entropy — impact −2.0
- `src/WhatToCook.Web/Components/Features/Recipes/Editor/Pages/RecipeEditorPage.razor.cs` — change entropy — impact −2.0
- `src/WhatToCook.Web/Components/Features/Recipes/Editor/Pages/RecipeEditorPage.razor` — change entropy — impact −2.0

<!-- REPOWISE_AGENTS:END -->
