## Why

UX-03 was a verify-first finding: Blazor Interactive Server has a reconnect UI, but the audit could not establish whether brief SignalR interruptions lose unsaved recipe/planner form input.

## What Changes

- Exercise a real Chromium browser going offline for at least ten seconds and coming back without reloading.
- Cover recipe-editor and new-plan draft fields, reconnect overlay state, ability to save, and a single persisted result.
- Reuse existing Playwright bootstrap and form helpers. CI reproduction demonstrated that a focused recipe step retains its DOM text but not the server model because InputTextArea normally binds on blur; add a small oninput update for steps without a separate persistence layer.

## Non-Goals

No forced server restart, long-lived/offline draft persistence, change to SignalR retry intervals or homelab reverse-proxy configuration. This tests the temporary network drop/rejoin path, not circuit eviction after a prolonged outage.
