# AGENT-03 — Resolve broken agent skill references

## Why

The tracked `run-tests` skill tells agents to consult `platform-detection`, `filter-syntax`, `writing-mstest-tests`, `migrate-vstest-to-mtp`, and `mtp-hot-reload`, but none of those skills is installed in `.agents/skills`. This is a concrete navigation defect, not a reason for wholesale consolidation of overlapping skill descriptions.

## What Changes

- Replace nonexistent skill suggestions with self-contained sections already present in `run-tests`, the maintained CI workflow, and the installed `systematic-debugging` skill.
- Add a small Node test that validates actionable named skill links in `run-tests` against actual installed directories; run it in existing PR CI.
- Keep the generated OpenSpec skills and OMP command adapters unchanged.

## Non-goals

No agent hierarchy, new generator, new skill, application code, or deployment change. AGENT-03 remains conditional/partial until broader instruction-drift evidence is found.

## Additional review correction

The original skill also incorrectly equated **SDK 10** with native MTP command mode, contradicting this repository's actual VSTest-mode `dotnet test WhatToCook.slnx`. The correct discriminator is the command runner selection in `global.json`; MTP bridge in VSTest mode still requires `--`, while native MTP may use it optionally. Keep the correction in this existing skill, not a new instruction layer.
