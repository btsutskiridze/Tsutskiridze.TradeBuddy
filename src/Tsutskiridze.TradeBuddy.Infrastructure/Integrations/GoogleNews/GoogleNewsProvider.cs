using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews
{
    public class GoogleNewsProvider : IGoogleNewsProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleNewsProvider> _logger;

        public GoogleNewsProvider(HttpClient httpClient, ILogger<GoogleNewsProvider> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<GoogleNewsItem>?> GetNewsAsync(string symbol, int? limit = null)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var searcUrl = $"https://www.google.com/search?q={symbol}&tbm=nws&tbs=sbd:1&hl=en";

            _logger.LogDebug("searchUrl ==> {searchUrl}", searcUrl);

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                            "(KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36"
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(searcUrl,
                new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            try
            {
                var acceptAllButton = page.Locator("button[aria-label='Accept all']").First;
                if (await acceptAllButton.IsVisibleAsync())
                {
                    await acceptAllButton.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Consent popup not found or click failed: {err}", ex.Message);
            }

            var pageContent = await page.ContentAsync();
            _logger.LogDebug("Page HTML content:\n{pageContent}", pageContent);

            try
            {
                _logger.LogInformation("Parsing Google News for {Symbol}", symbol);
                return await ParseNewsAsync(page, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Google News");
                return null;
            }
        }

        private static async Task<List<GoogleNewsItem>?> ParseNewsAsync(IPage page, int? limit)
        {
            await page.WaitForSelectorAsync("div.SoaBEf");

            var newsList = new List<GoogleNewsItem>();

            var newsItems = await page.QuerySelectorAllAsync("div.SoaBEf");
            foreach (var item in newsItems)
            {
                var titleDiv = await item.QuerySelectorAsync("div[class='n0jPhd ynAwRc MBeuO nDgy9d']");
                var urlAnchor = await item.QuerySelectorAsync("a");
                var summaryDiv = await item.QuerySelectorAsync("div[class='GI74Re nDgy9d']");
                var publishTimeDiv = await item.QuerySelectorAsync("div[class='OSrXXb rbYSKb LfVVr']");

                if (titleDiv == null || urlAnchor == null || summaryDiv == null || publishTimeDiv == null)
                {
                    continue;
                }

                var title = await titleDiv.InnerTextAsync() ?? "N/A";
                var url = await urlAnchor.GetAttributeAsync("href") ?? "N/A";
                var summary = await summaryDiv.InnerTextAsync() ?? "N/A";
                var publishTime = await publishTimeDiv.InnerTextAsync() ?? "N/A";

                if (url.StartsWith("/url?q="))
                    url = url.Substring(7).Split("&")[0];

                newsList.Add(new GoogleNewsItem
                {
                    Title = title,
                    Url = url,
                    Summary = summary,
                    PublishTime = ParsePublishTime(publishTime)
                });

                if (newsList.Count >= limit)
                {
                    break;
                }
            }

            return newsList.Count > 0 ? newsList : null;
        }

        private static string ParsePublishTime(string publishTime)
        {
            if (publishTime.ToLowerInvariant().Contains("ago"))
            {
                var parts = publishTime.Split(' ');
                if (parts.Length >= 3 && int.TryParse(parts[0], out int value))
                {
                    DateTime now = DateTime.Now;
                    string unit = parts[1].ToLowerInvariant();

                    if (unit.StartsWith("hour"))
                        return now.AddHours(-value).ToString("yyyy-MM-ddTHH:mm:ssZ");
                    else if (unit.StartsWith("day"))
                        return now.AddDays(-value).ToString("yyyy-MM-ddTHH:mm:ssZ");
                    else if (unit.StartsWith("week"))
                        return now.AddDays(-7 * value).ToString("yyyy-MM-ddTHH:mm:ssZ");
                    else if (unit.StartsWith("minute"))
                        return now.AddMinutes(-value).ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }
            else
            {
                if (DateTime.TryParse(publishTime, out DateTime absoluteDate))
                    return absoluteDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            return publishTime;
        }
    }
}

