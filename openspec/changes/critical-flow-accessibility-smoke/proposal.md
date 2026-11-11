# UX-02 — Accessibility checks for critical flows

## Why
The existing Playwright suite checks features and responsiveness but does not gate on automated WCAG checks. The most critical interactions must also be usable with keyboard-only navigation.

## Scope
Run a pinned axe-core Playwright scanner on the library, import and shopping entry routes at desktop and mobile viewports. Add focused keyboard checks for library search and import URL form. Fail CI on scanner violations; do not blanket-disable axe rules.

## Limits
Automated scanning cannot establish full WCAG 2.2 AA compliance. Dialog/drawer focus trapping, manual screen-reader exploration and reduced-motion testing remain additional work; implement those in follow-up PRs after a targeted interaction audit rather than claiming coverage here.
