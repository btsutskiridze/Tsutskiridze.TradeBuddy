using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Services.News;
using Microsoft.Playwright;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;

namespace Tsutskiridze.TradeBuddy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ApiControllerBase
    {

        private readonly NewsService _news;
        private readonly ILogger<NewsController> _logger;

        public NewsController(NewsService news, ILogger<NewsController> logger)
        {
            _news = news;
            _logger = logger;
        }


        [HttpGet("reddit/{symbol}")]
        public async Task<IActionResult> GetRedditPosts(string symbol, [FromQuery] int limit = 10, [FromQuery] string sortType = "new")
        {
            RedditSortType sort = Enum.TryParse<RedditSortType>(sortType, true, out var sortTypeEnum) ? sortTypeEnum : RedditSortType.New;

            var posts = await _news.GetRedditNews(symbol, sort, limit);

            return JsonResult(posts);
        }

        [HttpGet("google/{symbol}")]
        public async Task<IActionResult> GetGoogleNews(string symbol, [FromQuery] int limit = 10)
        {
            var news = await _news.GetGoogleNews(symbol, limit);

            return JsonResult(news);
        }

        [HttpGet("yahoo/{symbol}")]
        public async Task<IActionResult> GetYahooNews(string symbol, [FromQuery] int limit = 10)
        {
            // Initialize Playwright and launch the browser
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            // Create a new browser context/page
            var page = await browser.NewPageAsync();

            // Navigate to the Yahoo Finance news page for the given symbol in English
            // This query string helps ensure English content: ?lang=en-US&region=US
            var newsUrl = $"https://finance.yahoo.com/quote/{symbol}/news?lang=en-US&region=US";
            _logger.LogInformation("Navigating to: {NewsUrl}", newsUrl);
            await page.GotoAsync(newsUrl);

            // Wait for the news sections to be present (or time out if none appear)
            await page.WaitForSelectorAsync("section[data-testid='storyitem']", new PageWaitForSelectorOptions
            {
                Timeout = 10000 // 10 seconds
            });

            // log page html
                    // Log the full page HTML
            var fullHtml = await page.ContentAsync();
            _logger.LogInformation("Full HTML for {Symbol}:\n{FullHtml}", symbol, fullHtml);

            // Select all story items
            var storyItems = page.Locator("section[data-testid='storyitem']");
            var count = await storyItems.CountAsync();
            _logger.LogInformation("Found {Count} news items for {Symbol}", count, symbol);

            var newsList = new List<YahooNews>();

            for (int i = 0; i < count; i++)
            {
                // Check if we've reached the requested limit
                if (newsList.Count >= limit)
                {
                    break;
                }

                try
                {
                    var item = storyItems.Nth(i);

                    // Title
                    var titleElement = item.Locator("h3");
                    var title = await titleElement.InnerTextAsync() ?? "N/A";

                    // URL
                    var anchorElement = item.Locator("a.subtle-link");
                    var href = await anchorElement.GetAttributeAsync("href") ?? "";
                    var newsUrlFull = href.StartsWith("https", StringComparison.OrdinalIgnoreCase)
                        ? href
                        : $"https://finance.yahoo.com{href}";

                    // Summary
                    var summaryElement = item.Locator("p");
                    var summary = (await summaryElement.InnerTextAsync())?.Trim() ?? "N/A";

                    // Publish time
                    var timeElement = item.Locator("div[class*='publishing']");
                    var publishTime = (await timeElement.InnerTextAsync())?.Trim() ?? "N/A";

                    newsList.Add(new YahooNews
                    {
                        Title = title.Trim(),
                        Url = newsUrlFull.Trim(),
                        Summary = summary,
                        PublishTime = publishTime
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse a news item at index {Index}", i);
                }
            }

            // Close the browser
            await browser.CloseAsync();

            if (newsList.Count > 0){
                return JsonResult(newsList);
            }

            return JsonResult(fullHtml);
        }


        [HttpGet("finnhub/{symbol}")]
        public async Task<IActionResult> GetFinnhubNews(
            string symbol,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int limit = 10
        )
        {
            from = from == DateTime.MinValue ? DateTime.UtcNow.AddDays(-7) : from;
            to = to == DateTime.MinValue ? DateTime.UtcNow : to;

            var news = await _news.GetFinnhubNews(symbol, from, to, limit);

            return JsonResult(news);
        }
    }
}
