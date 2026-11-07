## Why

COR-03: Shopping generation tracks descriptive repetition but drops the count, and ignores meal-plan multipliers. A household may see one "szczypta" where two occurrences are planned.

## What Changes

- Preserve original descriptive quantity text and persist an independent, bounded context indicating occurrences and summed plan multipliers.
- Render that context in API copy text and the Blazor shopping checklist, including ingredients without an explicit quantity.
- Keep numeric quantity aggregation unchanged and do not multiply nonnumeric phrases such as "do smaku".

## Non-Goals

No new quantity arithmetic, inventory model, conversion across units, or full Shopping List 2.0 release.
