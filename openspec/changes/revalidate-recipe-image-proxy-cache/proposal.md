# PERF-06 (partial): Revalidate image responses through Web proxy

## Why
The API serves recipe photos through ASP.NET Core static-file middleware, with file validators (ETag and Last-Modified). The Blazor Web proxy currently sends unconditional upstream GETs, discards validators, and maps an upstream 304 to 404. Consequently browsers cannot revalidate photos using the API validators and a conditional GET is mishandled.

## Change
- Forward only the browser's `If-None-Match` and `If-Modified-Since` to the upstream API.
- Expose upstream ETag and Last-Modified, set a **private, must-revalidate** cache policy (`private, no-cache`) and preserve HTTP 304 without copying an image body.
- Extend existing upload-photo browser test to exercise a 200 followed by conditional 304 through Web.
- Record manual real-image network baseline instructions. Do **not** generate image variants absent representative transfer evidence.

## Non-goals
No image reencoding, resizing, new storage paths, CDN, authentication model, persistent cache, or changes to original upload validation. Do not claim a measured reduction in production transfer or LCP.

## Risk
Test actual 200/304 and ETag behavior end-to-end. Treat stale images and conditional revalidation as compatibility concerns. A 304 response must carry no body or incorrect 404 status.
