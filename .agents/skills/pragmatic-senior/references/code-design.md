# Code and design decisions

Use this reference when deciding whether a specific guard, layer, abstraction, or dependency belongs in a change.

## Frequent traps and exceptions

The right-hand column applies only when you can name the concrete case, not because the category sounds plausible.

| Avoid by default | Keep when you can name the case |
| --- | --- |
| Guards for states impossible under enforced invariants | A specific input from outside your control, a known legacy record shape, or an identified race |
| Revalidating unchanged data in every internal layer | A distinct risk at another boundary (for example, input validation plus a DB constraint for concurrent writes) |
| `catch` → log → throw, swallowing errors, or silently substituting defaults | Adding context, cleanup, retry for a known transient error, translating a boundary error, or explicit recovery |
| Unused flags, wrappers, helpers, interfaces, factories, or extra layers | A real boundary, current variation, contract, or responsibility that they simplify |
| Hypothetical old clients or migrations no one uses | Compatibility with existing public consumers, stored data, integration messages, and deployed versions |
| Comments repeating syntax | Explanation of non-obvious constraints, invariants, and architectural decisions |
| Third-party libraries for trivial built-in operations | Dependencies with tangible maintenance, correctness, or security benefits |

This is a default for unrequested complexity, not a veto against requirements the user has knowingly chosen.

## Duplication and abstraction

**Count and meaning both matter.** If the same business rule is implemented independently in **three or more places**, consolidate it by default using the smallest existing home: a local function, an existing class, or a simple helper. Do not introduce interfaces, factories, base classes, or a configurable pipeline unless current use actually needs them. At **two copies**, do not extract automatically; an exception is a shared invariant that must remain synchronized or a change that truly requires centralization.

Two blocks with similar syntax but distinct responsibilities or change patterns are not duplication worth abstracting. Conversely, three implementations of the same rule should not drift separately merely to keep the diff small.

**Scrutinize any abstraction.** Ask what concrete responsibility it hides, what coupling it reduces, or what real variation it enables. If a wrapper, adapter, mapper, service, factory, helper, inheritance layer, or pipeline only forwards to another component, collapse it when contracts and callers allow. A useful abstraction can add lines and still reduce total maintenance cost.

Extract from a long method only when separation improves comprehension or isolates a genuine responsibility; do not create a chain of one-line helpers. Retain tests that cover still-required behavior after consolidation.

### Good and bad decisions

- **Good:** Three handlers duplicate the same pricing rule. Replace all three with one well-named method on an existing suitable component. **Bad:** introduce `IRuleStrategyFactory`, three strategies, and registration configuration for that rule.
- **Good:** Two similar pieces of formatting have different domain meanings and evolve independently. Leave them separate. **Bad:** force them through a generic helper with booleans and optional arguments.
- **Good:** A facade only forwards one call to a service and no consumer or boundary requires it. Inline the forwarding and delete the facade after checking usages. **Bad:** replace it with another adapter just to keep the layer count unchanged.
- **Good:** An API checks request shape while a DB `UNIQUE` constraint enforces integrity during concurrent writes. Keep both. **Bad:** delete the constraint as "duplicated validation."

## Two scenarios to calibrate judgment

**Application:** A handler already validates a request at the API boundary. A new internal method receives the validated typed value. Repeating the same validation, wrapping every call in catch/log/rethrow, and adding a generic validator provider are unnecessary. A targeted check is justified only if you can point to the source of bad values, for example a column that older rows populated before the rule existed.

**Infrastructure:** A Compose change introduces a new service that relies on a VPN gateway. A compact config is good, but removing a network isolation rule, health dependency, or restore check merely to simplify YAML removes protection against a failure you can name. Keep those controls; avoid a new orchestration framework when existing tooling suffices.

## Decisions worth documenting

For public APIs, persistent data, security boundaries, and deployment contracts, record the chosen approach, rejected material alternative, why it fits now, and how to reverse or migrate. For a local reversible detail, prefer a concise code change over an ADR.
