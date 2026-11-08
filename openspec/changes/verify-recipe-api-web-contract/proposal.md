# ARC-05 — Verify the recipe API/Web consumer contract

## Why

WhatToCook runs the API and Blazor Web as separate deployable services with feature-local DTOs. The recipe catalog consumer currently uses default-initialized fields; if the API omits a required page total, recipe title, or details steps after a refactor, JSON deserialization can appear successful and the UI can show incorrect empty content.

## What changes

- Add `[JsonRequired]` only to mandatory fields of the Web's private recipe page/summary/details DTOs. Keep optional source/photo fields optional.
- Verify `page` and `pageSize` returned by the server match the requested page. This also prevents the planner's multi-page traversal from silently treating the wrong page as correct.
- Add focused negative tests for omitted required fields and wrong page metadata. Update the existing planner HTTP fixture to include explicit page metadata.
- Add one live API HTTP integration test for the fields and JSON shapes consumed by the Web's recipe catalog and details flows.

## Non-goals

No shared DTO assembly, SDK-generated global client, database migration, route changes or blanket snapshot of all endpoints. Other modules' contracts remain outside this targeted regression and should be covered when their consumer risk warrants it.

## Risk and verification

The API currently produces the required properties. `[JsonRequired]` detects **missing properties**; it does not by itself guarantee non-null values. Invalid JSON remains an error surfaced by existing client error handling. Run existing full CI including Playwright to verify normal responses keep deserializing.
