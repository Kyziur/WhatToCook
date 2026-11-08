import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { test } from "node:test";
import { evaluateNuGetAudit } from "../../scripts/ci/check-nuget-audit.mjs";

const reportWith = (topLevelPackages = [], transitivePackages = []) => ({
    version: 1,
    projects: [
        {
            path: "src/WhatToCook.Api/WhatToCook.Api.csproj",
            frameworks: [
                { framework: "net10.0", topLevelPackages, transitivePackages },
            ],
        },
    ],
});

const vulnerable = (severity) => ({
    id: "Fixture.Package",
    resolvedVersion: "1.0.0",
    vulnerabilities: [
        { severity, advisoryurl: "https://github.com/advisories/GHSA-fixture" },
    ],
});

test("clean report without frameworks passes", () => {
    const report = { version: 1, projects: [{ path: "test.csproj" }] };
    assert.deepEqual(evaluateNuGetAudit(report), {
        findings: [],
        blockingFindings: [],
    });
});

test("low and moderate vulnerabilities remain visible without blocking", () => {
    const result = evaluateNuGetAudit(
        reportWith([vulnerable("Low")], [vulnerable("Moderate")])
    );
    assert.equal(result.findings.length, 2);
    assert.equal(result.blockingFindings.length, 0);
});

test("high and critical vulnerabilities fail the gate, including transitives", () => {
    const result = evaluateNuGetAudit(
        reportWith([vulnerable("High")], [vulnerable("Critical")])
    );
    assert.equal(result.blockingFindings.length, 2);
    assert.deepEqual(
        result.blockingFindings.map((finding) => finding.severity),
        ["high", "critical"]
    );
});

test("empty, malformed, and changed-schema results fail closed", () => {
    for (const report of [
        null,
        {},
        { version: 2, projects: [{ path: "test.csproj" }] },
        { version: 1, projects: [] },
        { version: 1, projects: [{ path: "test.csproj", frameworks: {} }] },
        reportWith([vulnerable("Unknown")]),
        reportWith([{ id: "Package", resolvedVersion: "1", vulnerabilities: {} }]),
    ]) {
        assert.throws(() => evaluateNuGetAudit(report));
    }
});

test("CLI exits nonzero for blocking and unreadable reports, but zero for clean ones", () => {
    const dir = mkdtempSync(join(tmpdir(), "wtc-audit-gate-"));
    const script = fileURLToPath(
        new URL("../../scripts/ci/check-nuget-audit.mjs", import.meta.url)
    );
    try {
        for (const [name, report, expectedExit] of [
            ["clean", JSON.stringify(reportWith()), 0],
            ["high", JSON.stringify(reportWith([vulnerable("High")])), 1],
            ["malformed", "{bad-json", 1],
        ]) {
            const path = join(dir, `${name}.json`);
            writeFileSync(path, report);
            const result = spawnSync(process.execPath, [script, path], {
                encoding: "utf8",
            });
            assert.equal(
                result.status,
                expectedExit,
                `Unexpected exit for ${name}: ${result.stderr}`
            );
            if (expectedExit) {
                assert.match(result.stderr, /NuGet dependency audit gate failed/);
            }
        }
    } finally {
        rmSync(dir, { recursive: true, force: true });
    }
});
