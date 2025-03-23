using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Services.News;

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
        public async Task<IActionResult> GetYahooNews(string symbol = "NVDA", [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            // Navigate to the Yahoo Finance news page for the given symbol in English
            // This query string helps ensure English content: ?lang=en-US&region=US
            var searchUrl = $"https://finance.yahoo.com/quote/{symbol}/news?lang=en-US&region=US";
            _logger.LogInformation("Navigating to: {NewsUrl}", searchUrl);


            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                           "(KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36"
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(searchUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            try
            {
                // Locate the button (for example, a button with name "agree")
                var acceptAllButton = page.Locator("button[name='agree']").First;
                // Attempt to remove the overlay.
                var overlayLocator = page.Locator("div.scroll-down-wrapper.show");
                if (await overlayLocator.IsVisibleAsync())
                {
                    await overlayLocator.EvaluateAsync("element => element.remove()");
                }

                // Now click the button
                await acceptAllButton.ClickAsync(new LocatorClickOptions { Timeout = 3000 });
            }
            catch (Exception ex)
            {
                // If the button isn't found or the click fails within 1 second, log the error and continue
                _logger.LogError("Consent popup not found or click failed: {err}", ex.Message);
            }

            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var innerHtml = await page.ContentAsync();

            await browser.CloseAsync();

            return new ContentResult
            {
                Content = innerHtml,
                ContentType = "text/html",
            };

            //try
            //{
            //    await page.WaitForSelectorAsync("section[data-testid='storyitem']");

            //    // Select all story items
            //    var storyItems = page.Locator("section[data-testid='storyitem']");
            //    var count = await storyItems.CountAsync();
            //    _logger.LogInformation("Found {Count} news items for {Symbol}", count, symbol);

            //    var newsList = new List<YahooNews>();
            //    var htmlList = new List<string>();


            //    for (int i = 0; i < count; i++)
            //    {
            //        // Check if we've reached the requested limit
            //        if (newsList.Count >= limit)
            //        {
            //            break;
            //        }

            //        htmlList.Add(await storyItems.Nth(i).InnerHTMLAsync());

            //        cancellationToken.ThrowIfCancellationRequested();


            //        //try
            //        //{
            //        //    var item = storyItems.Nth(i);

            //        //    // Title
            //        //    var titleElement = item.Locator("h3").First;
            //        //    var title = await titleElement.InnerTextAsync() ?? "N/A";

            //        //    // URL
            //        //    var anchorElement = item.Locator("a.subtle-link").First;
            //        //    var href = await anchorElement.GetAttributeAsync("href") ?? "";
            //        //    var newsUrlFull = href.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            //        //        ? href
            //        //        : $"https://finance.yahoo.com{href}";

            //        //    // Summary
            //        //    var summaryElement = item.Locator("p").First;
            //        //    var summary = (await summaryElement.InnerTextAsync())?.Trim() ?? "N/A";

            //        //    // Publish time
            //        //    var timeElement = item.Locator("div[class*='publishing']").First;
            //        //    var publishTime = ((await timeElement.InnerTextAsync())?.Trim())?.Split("•\n").Last() ?? "N/A";

            //        //    newsList.Add(new YahooNews
            //        //    {
            //        //        Title = title.Trim(),
            //        //        Url = newsUrlFull.Trim(),
            //        //        Summary = summary,
            //        //        PublishTime = publishTime
            //        //    });
            //        //}
            //        //catch (Exception ex)
            //        //{
            //        //    _logger.LogError(ex, "Failed to parse a news item at index {Index}", i);
            //        //    _logger.LogInformation(await storyItems.Nth(i).All());
            //        //}
            //    }

            //    // Close the browser
            //    await browser.CloseAsync();
            //    return JsonResult(htmlList);

            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Failed to parse Yahoo News");
            //    cancellationToken.ThrowIfCancellationRequested();
            //    return null;
            //}
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
