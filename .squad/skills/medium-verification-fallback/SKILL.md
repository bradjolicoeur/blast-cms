---
name: "medium-verification-fallback"
description: "Handle Medium or Cloudflare verification interstitials without making the scraper path heavy"
domain: "content-capture, error-handling"
confidence: "high"
source: "earned"
---

## Context
Use this when a browser-based article capture path sometimes lands on a short-lived bot-check or Cloudflare verification page before the real article becomes available.

## Patterns
- Keep the browser path lightweight: reuse the existing browser/context and block heavy resource types.
- Do not trust the first rendered DOM snapshot. Poll briefly for the interstitial to clear before extracting content.
- Detect interstitials with reusable formatter helpers based on verification language plus challenge markers.
- Add one browser-header HTTP fallback when the browser path still looks challenged.
- If every capture path still looks like verification HTML, return empty content rather than challenge text.

## Examples
- Playwright wait + fallback: `src\blastcms.ArticleScanService\CaptureMeta\CaptureMediumArticleMeta.cs`
- Shared browser-header HTTP loader: `src\blastcms.ArticleScanService\CaptureMeta\CaptureHtmlPageMeta.cs`
- Interstitial detection helpers: `src\blastcms.ArticleScanService\CaptureMeta\CaptureMetaContentFormatter.cs`
- Regression tests: `src\blastcms.web.tests\Services\CaptureMetaContentFormatterTests.cs`

## Anti-Patterns
- Spinning up extra browsers or persistent browser state as the first fix
- Treating challenge HTML as "better than nothing"
- Depending on request logs alone to decide an interstitial happened
