import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { test } from "node:test";

// These entry points are maintained by their respective OpenSpec harness adapters.
// We assert shared safety invariants, not byte-for-byte content or command names.
const apply = [
  ".omp/commands/opsx-apply.md",
  ".agents/skills/openspec-apply-change/SKILL.md",
];
const archive = [
  ".omp/commands/opsx-archive.md",
  ".agents/skills/openspec-archive-change/SKILL.md",
];

const invariants = {
  apply: [
    ["Required live context", /Treat `context` as a required prompt-level input/],
    ["Complete-task only", /Only mark a task `- \[x\]` when its specified behavior is fully implemented/],
    ["CLI state is authoritative", /Preserve CLI-controlled blocked\/ready\/all-done behavior/],
    ["Do not copy runtime guidance", /Do not copy runtime context or operation guidance into implementation files or planning artifacts/],
  ],
  archive: [
    ["Never archive during in-flight sync", /Never archive while a spec sync is still in flight/],
    ["Reject incomplete synchronization", /If the sync failed, or any capability does not match/],
    ["Inspect all delta specs before archiving", /If delta specs exist, always run the sync assessment/],
    ["Do not override archive paths from advisory guidance", /Do not infer replacement paths, skipped prompts, or flags from/],
  ],
};

for (const [workflow, paths] of Object.entries({ apply, archive })) {
  test(`${workflow} harnesses retain shared OpenSpec safety invariants`, () => {
    for (const path of paths) {
      const text = readFileSync(path, "utf8");
      for (const [label, pattern] of invariants[workflow]) {
        assert.match(text, pattern, `${path} is missing invariant: ${label}`);
      }
    }
  });
}
