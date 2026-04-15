using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public sealed class CaptureMediumArticleMeta(ILogger<CaptureMediumArticleMeta> logger) : ICaptureMeta, IAsyncDisposable
    {
        private const int NavigationTimeoutMs = 20_000;
        private const int ArticleWaitTimeoutMs = 8_000;
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

                var html = await page.ContentAsync();
                var result = CaptureMetaContentFormatter.FormatHtml(html);

                logger.LogInformation("Captured rendered Medium content from: {url}", url);
                return result;
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
