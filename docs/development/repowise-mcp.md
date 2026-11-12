# Repowise: optional local code index

Use Repowise when a current repository overview helps. It is **advisory**, never a replacement for reading source and tests.

The committed [`.mcp.json`](../../.mcp.json) runs `repowise mcp --transport stdio` without a machine-specific checkout path. Install Repowise on the developer machine, initialize the current checkout with `repowise init`, and ensure the MCP client launches from **that checkout's root**. A different cwd can select the wrong index; use an uncommitted personal absolute-path override if needed.

## Check freshness before trusting a result

Run from the active checkout (PowerShell or another shell):

```powershell
git rev-parse HEAD
repowise status --format json
```

The first command prints the current commit; the second reports the local Repowise index state. Compare both with the **live response's** indexed revision, source scope and approximation/staleness flags. If stale, uncertain or pointing at another worktree, read current code and OpenSpec directly. Do not edit the generated Repowise region of `AGENTS.md`.

```powershell
node --test tests/ci/repowise-config.test.mjs
```

This checks only the portable committed configuration; **it does not test** whether the installed client uses the right cwd or a fresh index. Verify that manually in each distinct checkout/WSL environment.
