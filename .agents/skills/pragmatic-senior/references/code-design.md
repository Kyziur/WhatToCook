# Code and design decisions

Consult when deciding whether to keep, remove, or introduce implementation complexity.

## Common traps

| Usually avoid | Keep when there is a demonstrated need |
| --- | --- |
| Guards for states excluded by enforced contracts | A reachable legacy input, trust-boundary violation, or identified race |
| Repeating the same validation in internal layers | A different failure mode with its own enforcement boundary |
| Catch/log/rethrow or silent defaults | Recovery, cleanup, translating an error contract, or adding useful context |
| Unused flags or indirection-only abstractions | A real responsibility, contract, or current variation |
| Imaginary compatibility shims | Existing consumers, deployed versions, or stored formats |
| Comments restating code | A non-obvious invariant, constraint, or design decision |
| Extra libraries for simple built-in operations | Material correctness, security, or maintenance benefits |

An exception requires a reachable scenario, not a hypothetical category.

## Extraction and abstraction

Shared meaning and change cadence matter more than matching syntax. Apply the duplication threshold in [the main skill](../SKILL.md): consolidate actual repeated rules with a local or existing helper, not a configurable framework. Distinct responsibilities may look similar and still belong apart.

Review abstractions of any kind, including wrappers, interfaces, factories, adapters, mapping layers, and base classes. Ask what problem each solves today. Collapse forwarding-only layers when no boundary or consumer requires them. Split a large method only when distinct responsibilities or complex flow become clearer, not to enforce a line limit.

## Calibration examples

- Two modules format similar-looking data for different domain contracts. Keep them independent rather than adding a generic formatter with mode flags.
- A forwarding-only facade adds no contract or responsibility. Remove it and update callers if the repository's actual usage supports deletion.
- A configuration field's allowed values appear both in its schema and in a hand-written validation table. Derive the table from the schema the parser already uses, or enforce the constraint where the value is written, so the two lists cannot drift. Keep any check that guards a distinct, identified risk.
- A configuration simplification would remove a service's necessary network restriction. Preserve the control and simplify elsewhere, without inventing a new orchestration framework.

## Deletion evidence

Search live callers and inspect only the dynamic usage mechanisms present in this repository (such as its actual registration or serialization rules). A speculative list of every possible reflection or plugin technique is not a reason to keep unreferenced code. If external consumers cannot be inspected and usage is plausible, keep the public contract and disclose the uncertainty.

Retain tests for supported behavior after refactoring. For significant public contracts or persistent formats, note the decision and migration implications; local reversible cleanup usually needs no separate document.
