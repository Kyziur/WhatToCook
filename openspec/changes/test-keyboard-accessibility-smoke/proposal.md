## Why

UX-02: existing ARIA/component assertions do not prove that the most-used controls can be activated and traversed with a keyboard in a real browser.

## What Changes

- Add Playwright keyboard-only smoke coverage for mobile library disclosure and editor form order.
- Exercise both Enter and Space on a shopping-list item's existing pressed-state control.
- Reuse the existing Playwright fixtures/helpers; keep this first slice small and non-invasive.

## Scope

This change is the **keyboard-first slice** of UX-02, not a comprehensive WCAG 2.2 AA audit. Follow up with axe-based scanner coverage and focus management checks for dialogs/drawers once identified issues are prioritized.

## Verification

Playwright desktop/mobile; normal .NET tests, ARM64 and CSharpier CI.
