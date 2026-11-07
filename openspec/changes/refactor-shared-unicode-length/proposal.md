## Why

PostgreSQL varchar(n) counts Unicode scalar values, while .NET string.Length counts UTF-16 code units. Current input rules duplicate a Rune enumeration in three places and recipe writes still use string.Length.

## What Changes

- Introduce one small reusable Unicode scalar limit helper under API infrastructure, with optional trimming.
- Use it consistently for recipe writes, planning, manual shopping items, and import drafts without altering existing response contracts.
- Regression-test null, trimming, BMP/non-BMP, and max/max+1 boundaries.

## Non-Goals

- No new shared assembly or validation framework.
- No database migration, request shape change, or conversion to grapheme-cluster counting.

## Verification

Release build; .NET integration and unit tests; existing endpoint regression tests; CSharpier.
