---
name: verification-fast-path
description: Choose the fastest safe verification path for WhatToCook during implementation, then expand to the mandatory handoff sequence before considering a package complete.
license: MIT
---

# Verification Fast Path

Use this skill when the question is not "can we verify?" but "what is the narrowest meaningful verification first?".

## Modes

### Inner Loop

Use the smallest relevant checks for the changed surface:

- `src/WhatToCook.Api` only:
  - `dotnet build WhatToCook.slnx`
  - relevant `dotnet test` filter or affected test project
- `src/WhatToCook.Web` or component logic:
  - `dotnet build WhatToCook.slnx`
  - relevant component tests only when the component behavior changed
  - use the `agent-browser` skill for the changed user journey: inspect desktop and mobile viewports, exercise the primary interaction, and check browser console errors
- `tests/e2e` or browser flow changes:
  - use `agent-browser` to validate the browser flow while editing
  - defer Playwright E2E until the changed flow is stable; run a scoped subset only to diagnose an E2E-specific failure
- skills, prompts, tooling, or workflow-only changes:
  - run the narrowest meaningful smoke check for the affected tool
  - do not run application E2E unless the task also changes application behavior

### Final Handoff

Run this sequence once, immediately before the final handoff of the complete user-requested task. A delivery package is not an intermediate edit or a small fix within the same task.

1. `dotnet build WhatToCook.slnx`
2. `dotnet test WhatToCook.slnx` or intentional filtered scope
3. `PLAYWRIGHT_HTML_OPEN=never npx playwright test --config=playwright.config.ts`
4. `dotnet husky run`
5. If Husky skips because no files are staged: `dotnet csharpier format .`

## Guardrails

- Never run `dotnet build` and `dotnet test` in parallel in this repo.
- Do not run Playwright E2E after each small UI change; browser-assisted validation through `agent-browser` is the default UI inner loop.
- Do not run the full Playwright suite between small changes in the same task. Reserve it for final handoff.
- For skills, prompts, documentation, and workflow-only changes, run an affected-tool smoke check; do not run application E2E unless application behavior also changed.
- If final E2E fails, diagnose the changed journey with `agent-browser`, fix it, and rerun the same E2E scope before broadening.
- If the user explicitly accepts a blocker or intentionally narrower scope, say so in the final verification notes.

## Shell Timeouts

Pass an explicit timeout to every non-interactive verification command. Do not rely on the shell tool's 120-second default:

- `dotnet build`: 300000 ms
- `dotnet test`: 600000 ms
- scoped Playwright E2E: 300000 ms
- full Playwright E2E: 900000 ms
- Husky or CSharpier: 300000 ms

Start API, Web, database, and other local services with the tracked background-process tool and a readiness probe. If a command times out, examine its output and active processes first. Retry with a longer timeout only when it was making normal progress; never kill a process the current agent did not start.
