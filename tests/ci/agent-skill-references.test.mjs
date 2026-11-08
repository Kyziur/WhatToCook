import assert from "node:assert/strict";
import { existsSync, readFileSync } from "node:fs";
import { test } from "node:test";

test("run-tests skill points only to installed named skills", () => {
  const source = readFileSync(".agents/skills/run-tests/SKILL.md", "utf8");
  const references = [...source.matchAll(/`([a-z][a-z0-9-]+)`\s+skill\b/g)]
    .map((match) => match[1]);

  assert.ok(references.length > 0, "Expected at least one actionable skill reference");
  for (const name of references) {
    assert.ok(
      existsSync(`.agents/skills/${name}/SKILL.md`),
      `Unresolved skill reference: ${name}`,
    );
  }
});
