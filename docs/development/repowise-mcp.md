# Repowise MCP configuration (AGENT-04)

The committed [`.mcp.json`](../../.mcp.json) is portable: it starts `repowise mcp --transport stdio` **without an absolute repository path**. Repowise's documented no-path mode discovers the initialized `.repowise` directory by walking upward from the MCP process's current working directory.

## Preconditions

- Install the Repowise CLI on the **developer machine** and ensure `repowise` is on the MCP host's `PATH`.
- Initialize Repowise for each checkout using its current setup procedure (`repowise init` in the repository root). Keep local index/configuration and API keys out of git.
- Use a project-local MCP host that launches the subprocess with **cwd set to this checkout**. This is a required assumption, **not** a feature guaranteed by every MCP client.

## Smoke test for a checkout or worktree

In the checkout's root, `repowise mcp --transport stdio` should start the MCP stdio server against that checkout. Start/stop it through your MCP client, then request a repository overview and verify its reported path/index revision. Repeat from a second checkout location (and WSL if relevant) to ensure it does **not** silently select the earlier repository. Do not confuse a running MCP process with a fresh index.

If your MCP client does **not** use the project root as process cwd, use a **personal, noncommitted client config** with `repowise mcp /absolute/path/to/that/checkout --transport stdio`. Do not restore a machine-specific path in the committed file. An uninitialized checkout or missing CLI must be treated as a setup failure, not a successful portability test.

## Automated scope

`node --test tests/ci/repowise-config.test.mjs` verifies the committed JSON remains path-free, uses stdio, and points agents at the current Blazor stack. It does **not** launch Repowise or certify MCP-client cwd behavior. A real host smoke test remains necessary before treating AGENT-04 as complete across environments.

References: [Repowise MCP server documentation](https://github.com/repowise-dev/repowise/blob/main/website/mcp-server.md), [CLI reference](https://github.com/repowise-dev/repowise/blob/main/docs/reference/CLI_REFERENCE.md).

## Index freshness preflight (AGENT-01)

Run these commands **in the checkout that the MCP host is using**, not a different copy:

```sh
git rev-parse HEAD
repowise status --format json
```

`git rev-parse HEAD` prints the working checkout's committed revision. `repowise status --format json` shows the CLI's index/wiki sync state for that checkout. Before relying on a Repowise MCP response, also inspect its **live** freshness/approximation metadata and indexed revision/scope; do not infer live freshness from the checked-in generated `AGENTS.md` footer or its old `confidence` number. If an exact revision is unavailable, behind HEAD, or the response is marked approximate/stale, inspect the **current source and specs directly** before applying changes.

The human-maintained instructions above the generated region in `AGENTS.md` take precedence if that region says to trust its index without rereading. Do not manually patch the generated section; refresh Repowise using its supported workflow when appropriate. This is a **per-task preflight**, not a recurring manual CI check and not proof of AGENT-04's second-checkout MCP-client behavior.
