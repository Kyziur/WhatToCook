# Diagrams

Read this when adding or reviewing a diagram. A diagram is optional: choose one only when it clarifies.

## Choose only when it clarifies

A diagram must answer a question more effectively than a paragraph. Select the smallest useful form:

| Question | Good choice |
| --- | --- |
| Who/what connects to this system? | C4 system context |
| Which services, applications or stores exist? | C4 container-level view |
| In what order do participants interact? | Sequence diagram |
| What decision path or operation should I follow? | Flowchart |
| Which states and transitions matter? | State diagram |
| Where does software run, and across which boundaries? | Deployment/topology diagram |

C4 describes **abstraction levels**, not a requirement to use C4-specific diagram syntax. Prefer editable Mermaid in Markdown if supported; a plain Mermaid flowchart can express a C4-like view. Reuse the repository's established diagram tools otherwise. Do **not** default to component- or code-level C4 views.

## Quality bar

Every included diagram should:

- Have a title or lead-in stating the question it answers.
- Name nodes by role and arrows by meaning; show direction.
- Respect the chosen abstraction level; omit irrelevant detail.
- Make relevant external, security and trust boundaries understandable.
- Distinguish **implemented/current**, **planned**, and **historical** states.
- Include a short text explanation so the page is useful without rendering.
- Render in the target Markdown environment where verification tools are available, and match its semantics against the actual source.

Never invent system edges, topology, trust assumptions, or deployment state. Prefer one legible diagram over a giant all-services chart. Avoid decorative diagrams, oversized edge labels, screenshots of editable diagrams, and unverified generated visuals.
