import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { test } from "node:test";

const auditPath = "docs/audits/2026-11-02-repository-audit.md";
const expectedOriginalFindings = 69;

function countStatuses(statuses) {
  const counts = { done: 0, partial: 0, review: 0, open: 0 };

  for (const status of statuses) {
    if (status === "**DONE · CI**") counts.done++;
    else if (["**PARTIAL**", "**PARTIAL · CI**", "**PARTIAL · IN REVIEW**"].includes(status))
      counts.partial++;
    else if (status === "**IN REVIEW**") counts.review++;
    else if (status === "OPEN / CONDITIONAL") counts.open++;
    else throw new Error(`Unknown audit status: ${status}`);
  }

  return counts;
}

test("the original audit contains 69 unique finding rows", () => {
  const markdown = readFileSync(auditPath, "utf8");
  const rows = [...markdown.matchAll(/^\|\s*([A-Z]+-\d+)\s*\|\s*([^|]+?)\s*\|/gm)];
  const ids = rows.map((row) => row[1]);

  assert.equal(ids.length, expectedOriginalFindings, "Original finding count changed");
  assert.equal(new Set(ids).size, ids.length, "Duplicate finding identifiers in audit");
  assert.ok(ids.every((id) => /^[A-Z]+-\d{2}$/.test(id)));
});

test("the current audit summary equals the actual finding statuses", () => {
  const markdown = readFileSync(auditPath, "utf8");
  const rows = [...markdown.matchAll(/^\|\s*([A-Z]+-\d+)\s*\|\s*([^|]+?)\s*\|/gm)];
  const actual = countStatuses(rows.map((row) => row[2].trim()));
  const summary = markdown.match(
    /^\*\*Current count:\*\* (\d+) DONE · CI, (\d+) PARTIAL, (?:(\d+) IN REVIEW, )?(\d+) open\/conditional\./m
  );

  assert.ok(summary, "Missing or malformed current audit summary");
  const expected = {
    done: Number(summary[1]),
    partial: Number(summary[2]),
    review: Number(summary[3] ?? 0),
    open: Number(summary[4]),
  };
  assert.deepEqual(actual, expected, "Update summary counts whenever changing finding statuses");
  assert.equal(Object.values(actual).reduce((sum, value) => sum + value, 0), expectedOriginalFindings);
});

test("in-review partial findings remain counted only as partial", () => {
  assert.deepEqual(
    countStatuses(["**PARTIAL · IN REVIEW**", "**IN REVIEW**", "OPEN / CONDITIONAL"]),
    { done: 0, partial: 1, review: 1, open: 1 }
  );
});
