import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { fileURLToPath } from "node:url";

const severityOrder = { low: 1, moderate: 2, high: 3, critical: 4 };
const blockingThreshold = severityOrder.high;

export function evaluateNuGetAudit(report) {
    if (
        !report ||
        typeof report !== "object" ||
        report.version !== 1 ||
        !Array.isArray(report.projects) ||
        report.projects.length === 0
    ) {
        throw new Error("NuGet audit report has no valid version-1 project results.");
    }

    const findings = [];
    for (const project of report.projects) {
        if (!project || typeof project.path !== "string" || !project.path.trim()) {
            throw new Error("NuGet audit report contains an invalid project.");
        }
        // The CLI omits frameworks when the project has no vulnerable packages.
        if (project.frameworks !== undefined && !Array.isArray(project.frameworks)) {
            throw new Error("NuGet audit report has an invalid frameworks collection.");
        }

        for (const framework of project.frameworks ?? []) {
            if (!framework || typeof framework !== "object") {
                throw new Error("NuGet audit report contains an invalid framework.");
            }
            for (const kind of ["topLevelPackages", "transitivePackages"]) {
                if (framework[kind] !== undefined && !Array.isArray(framework[kind])) {
                    throw new Error(`NuGet audit report has an invalid ${kind} collection.`);
                }
                for (const pkg of framework[kind] ?? []) {
                    if (
                        !pkg ||
                        typeof pkg.id !== "string" ||
                        !pkg.id.trim() ||
                        typeof pkg.resolvedVersion !== "string"
                    ) {
                        throw new Error("NuGet audit report contains an invalid package.");
                    }
                    if (pkg.vulnerabilities !== undefined && !Array.isArray(pkg.vulnerabilities)) {
                        throw new Error("NuGet audit report has an invalid vulnerabilities collection.");
                    }
                    for (const vulnerability of pkg.vulnerabilities ?? []) {
                        const severity = vulnerability?.severity?.toLowerCase();
                        if (!Object.hasOwn(severityOrder, severity)) {
                            throw new Error("NuGet audit report contains an unknown vulnerability severity.");
                        }
                        findings.push({
                            project: project.path,
                            packageId: pkg.id,
                            version: pkg.resolvedVersion,
                            severity,
                            advisory: vulnerability.advisoryurl ?? "",
                        });
                    }
                }
            }
        }
    }
    return {
        findings,
        blockingFindings: findings.filter(
            (finding) => severityOrder[finding.severity] >= blockingThreshold
        ),
    };
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
    try {
        if (process.argv.length !== 3) {
            throw new Error("Usage: node check-nuget-audit.mjs <report.json>");
        }
        const report = JSON.parse(readFileSync(process.argv[2], "utf8"));
        const { findings, blockingFindings } = evaluateNuGetAudit(report);
        console.log(`NuGet audit: ${findings.length} advisory occurrences; ${blockingFindings.length} High/Critical.`);
        for (const finding of findings) {
            console.log(
                `[${finding.severity}] ${finding.project}: ${finding.packageId}@${finding.version} ${finding.advisory}`
            );
        }
        if (blockingFindings.length > 0) {
            console.error("NuGet dependency audit gate failed (High/Critical advisory found).");
            process.exitCode = 1;
        }
    } catch (error) {
        console.error(`NuGet dependency audit gate failed: ${error.message}`);
        process.exitCode = 1;
    }
}
