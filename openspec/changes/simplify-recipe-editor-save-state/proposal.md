# Simplify recipe editor saving state

## Why
Manual IsSaving resets are repeated across validation, API and photo-upload branches. A forgotten reset could strand the editor.

## Changes
Release IsSaving once in finally for every finished save attempt, without changing validation focus, feedback, photo retry, saved recipe identity or navigation. Test failure followed by successful retry.

## Boundaries
No new state service, API, schema, CSS, backend, deployment or circuit-recovery behavior.
