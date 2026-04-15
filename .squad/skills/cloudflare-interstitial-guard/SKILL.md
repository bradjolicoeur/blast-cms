---
name: "cloudflare-interstitial-guard"
description: "Detect and suppress Cloudflare verification HTML in article/meta capture paths"
domain: "testing, content-capture"
confidence: "high"
source: "earned"
---

## Context
Use this when a scraper or HTML formatter can receive a verification/interstitial page instead of the expected article content, especially on Medium-style capture flows. The goal is to stop bot-check copy from being treated as real title, description, or article body text.

## Patterns
- Reproduce the failure with a static HTML fixture, not a live browser/network dependency.
- Detect interstitials with a **compound signature**, not a single keyword:
  - verification language such as `checking your browser`, `verify you are human`, or `verifies users to protect the site from malicious bots`
  - plus a Cloudflare/challenge marker such as `Cloudflare`, `Just a moment...`, `cf-`/`challenge` ids or classes, or `/cdn-cgi/challenge-platform`
- Short-circuit formatting to an empty result for interstitial HTML so downstream summarizers do not ingest bogus content.
- On Medium/custom Medium domains, a **successful article render can still make** `/cdn-cgi/challenge-platform/...` requests. Do not classify from request logs alone; require DOM/title/body challenge copy as well.
- Member-only Medium preview/regwall text like `Member-only story` or `Create an account to read the full story.` is still a real article render, not a Cloudflare interstitial.

## Examples
- Production guard: `src\blastcms.ArticleScanService\CaptureMeta\CaptureMetaContentFormatter.cs`
- Regression fixture: `src\blastcms.web.tests\Services\CaptureMetaContentFormatterTests.cs`

## Anti-Patterns
- Do not depend on live Medium or Playwright runs to validate the behavior.
- Do not match on `Cloudflare` alone; require verification language too to reduce false positives.
- Do not leave interstitial meta descriptions in place "just in case"—they pollute downstream summaries.
