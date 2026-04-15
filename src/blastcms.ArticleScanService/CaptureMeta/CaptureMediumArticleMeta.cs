using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public sealed class CaptureMediumArticleMeta(
        ILogger<CaptureMediumArticleMeta> logger,
        IHttpClientFactory httpClientFactory) : ICaptureMeta, IAsyncDisposable
    {
        private const int NavigationTimeoutMs = 20_000;
        private const int ArticleWaitTimeoutMs = 8_000;
        private const int NetworkIdleTimeoutMs = 5_000;
        private const int VerificationPollDelayMs = 2_000;
        private const int VerificationPollAttempts = 6;
        private const int MinimumArticleTextLength = 80;
        private static readonly string[] BrowserArgs =
        [
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
        ];

        private readonly SemaphoreSlim _initLock = new(1, 1);
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;

        public async Task<CaptureMetaResult> GetMeta(string url)
        {
            var uri = new Uri(url);
            if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only http/https URLs are supported.", nameof(url));
            }

            logger.LogInformation("Scraping Medium article with Playwright: {url}", url);
            var context = await GetContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = NavigationTimeoutMs
                });

                try
                {
                    await page.WaitForSelectorAsync("article", new PageWaitForSelectorOptions
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = ArticleWaitTimeoutMs
                    });
                }
                catch (TimeoutException)
                {
                    logger.LogWarning("Timed out waiting for Medium article element on {url}; using rendered DOM snapshot.", url);
                }

                await WaitForVerificationToClearAsync(page, url);

                var html = await page.ContentAsync();
                if (CaptureMetaContentFormatter.TryFormatHtml(html, out var result))
                {
                    logger.LogInformation("Captured rendered Medium content from: {url}", url);
                    return result;
                }

                foreach (var fallbackUrl in GetFallbackUrls(page, url))
                {
                    logger.LogWarning(
                        "Medium page for {url} remained unreadable after Playwright capture; trying browser-header HTTP fallback via {fallbackUrl}.",
                        url,
                        fallbackUrl);

                    var fallbackResult = await TryFallbackCaptureAsync(fallbackUrl);
                    if (fallbackResult != null)
                    {
                        logger.LogInformation("Captured Medium content using HTTP fallback via {fallbackUrl}.", fallbackUrl);
                        return fallbackResult;
                    }
                }

                logger.LogWarning("Medium capture for {url} still resembled a verification interstitial after fallback; returning empty content.", url);
                return new CaptureMetaResult(string.Empty);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private async Task<IBrowserContext> GetContextAsync()
        {
            if (_context != null)
            {
                return _context;
            }

            await _initLock.WaitAsync();
            try
            {
                if (_context != null)
                {
                    return _context;
                }

                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH")))
                {
                    Microsoft.Playwright.Program.Main(["install", "chromium"]);
                }

                _playwright = await Playwright.CreateAsync();
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = BrowserArgs
                });

                _context = await _browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                              + "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
                });

                await _context.RouteAsync("**/*", async route =>
                {
                    var type = route.Request.ResourceType;
                    if (type is "image" or "media" or "font" or "stylesheet" or "ping" or "eventsource")
                    {
                        await route.AbortAsync();
                    }
                    else
                    {
                        await route.ContinueAsync();
                    }
                });

                return _context;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private async Task WaitForVerificationToClearAsync(IPage page, string url)
        {
            await WaitForLoadStateSafelyAsync(page, LoadState.NetworkIdle, NetworkIdleTimeoutMs);

            for (var attempt = 1; attempt <= VerificationPollAttempts; attempt++)
            {
                if (await HasReadableArticleAsync(page))
                {
                    return;
                }

                if (!await LooksLikeVerificationInterstitialAsync(page))
                {
                    return;
                }

                logger.LogInformation(
                    "Medium verification interstitial detected for {url}; waiting for article content ({attempt}/{maxAttempts}).",
                    url,
                    attempt,
                    VerificationPollAttempts);

                await page.WaitForTimeoutAsync(VerificationPollDelayMs);
                await WaitForLoadStateSafelyAsync(page, LoadState.NetworkIdle, NetworkIdleTimeoutMs);
            }
        }

        private static async Task WaitForLoadStateSafelyAsync(IPage page, LoadState loadState, float timeoutMs)
        {
            try
            {
                await page.WaitForLoadStateAsync(loadState, new PageWaitForLoadStateOptions
                {
                    Timeout = timeoutMs
                });
            }
            catch (TimeoutException)
            {
            }
        }

        private static async Task<bool> HasReadableArticleAsync(IPage page)
        {
            var articleLocator = page.Locator("article");
            try
            {
                if (await articleLocator.CountAsync() == 0)
                {
                    return false;
                }

                var article = articleLocator.First;
                var text = await article.TextContentAsync(new LocatorTextContentOptions
                {
                    Timeout = 1_000
                });

                return !string.IsNullOrWhiteSpace(text)
                    && text.Trim().Length >= MinimumArticleTextLength
                    && !CaptureMetaContentFormatter.LooksLikeVerificationInterstitialText(text);
            }
            catch (PlaywrightException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        private static async Task<bool> LooksLikeVerificationInterstitialAsync(IPage page)
        {
            try
            {
                var pageSignals = await page.EvaluateAsync<string>(
                    "() => [document.title ?? '', document.body?.innerText ?? '', document.body?.className ?? ''].join('\\n')");

                return CaptureMetaContentFormatter.LooksLikeVerificationInterstitialText(pageSignals);
            }
            catch (PlaywrightException)
            {
                return false;
            }
        }

        private async Task<CaptureMetaResult> TryFallbackCaptureAsync(string url)
        {
            try
            {
                var document = await CaptureHtmlPageMeta.LoadDocumentAsync(httpClientFactory, url);
                return CaptureMetaContentFormatter.TryFormatDocument(document, out var result) ? result : null;
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "HTTP fallback capture failed for Medium URL {url}.", url);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogWarning(ex, "HTTP fallback capture timed out for Medium URL {url}.", url);
                return null;
            }
        }

        private static string[] GetFallbackUrls(IPage page, string originalUrl)
        {
            var currentUrl = page.Url;
            if (string.IsNullOrWhiteSpace(currentUrl)
                || currentUrl.Equals(originalUrl, StringComparison.OrdinalIgnoreCase))
            {
                return [originalUrl];
            }

            return [currentUrl, originalUrl];
        }

        public async ValueTask DisposeAsync()
        {
            if (_context != null)
            {
                await _context.DisposeAsync();
            }

            if (_browser != null)
            {
                await _browser.DisposeAsync();
            }

            _playwright?.Dispose();
            _initLock.Dispose();
        }
    }
}
