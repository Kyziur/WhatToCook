import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { test } from "node:test";

const config = JSON.parse(readFileSync(".mcp.json", "utf8"));
const agentInstructions = readFileSync("AGENTS.md", "utf8");

test("Repowise MCP config does not pin a contributor checkout", () => {
  const repowise = config.mcpServers?.repowise;
  assert.equal(repowise?.command, "repowise");
  assert.deepEqual(repowise.args, ["mcp", "--transport", "stdio"]);
  assert.equal(repowise.args.some((arg) => arg.includes(":") || arg.startsWith("/")), false);
});

test("agent guidance acknowledges the real stack and stale generated context", () => {
  const manualSection = agentInstructions.split("<!-- REPOWISE_AGENTS:START")[0];
  assert.match(manualSection, /Blazor Interactive Server/);
  assert.match(manualSection, /current source/);
  assert.doesNotMatch(manualSection, /MAUI feature work/);
});

test("current-source priority overrides conflicting generated Repowise guidance", () => {
  const startMarker = "<!-- REPOWISE_AGENTS:START";
  const endMarker = "<!-- REPOWISE_AGENTS:END";
  const start = agentInstructions.indexOf(startMarker);
  const end = agentInstructions.indexOf(endMarker);
  assert.ok(start > 0 && end > start, "Repowise-generated region must be isolated");
  const maintained = agentInstructions.slice(0, start);
  assert.match(maintained, /take precedence/i);
  assert.match(maintained, /conflicting generated text/i);
  assert.match(maintained, /freshness check/i);
  assert.match(maintained, /indexed revision and freshness scope/i);
  assert.match(maintained, /re-read the current source/i);
});
