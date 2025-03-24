using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;
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
            
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36"
            });
            
            var page = await context.NewPageAsync();
            
            // 🚫 Block images, fonts, ads, etc. for performance
            await page.RouteAsync("**/*", async route =>
            {
                var type = route.Request.ResourceType;
                if (type is "image" or "font" or "stylesheet" or "media")
                    await route.AbortAsync();
                else
                    await route.ContinueAsync();
            });
            
            var searchUrl = $"https://finance.yahoo.com/quote/{symbol}/news?lang=en-US&region=US";
            _logger.LogInformation("Navigating to: {NewsUrl}", searchUrl);
            
            await page.GotoAsync(searchUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 10000
            });
            
            // 🧼 Try to accept consent popup
            try
            {
                var consentButton = page.Locator("button[name='agree']");
                if (await consentButton.IsVisibleAsync(new() { Timeout = 2000 }))
                {
                    await consentButton.ClickAsync();
                    _logger.LogInformation("Accepted Yahoo consent popup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Consent popup skipped: {err}", ex.Message);
            }
            
            // ⏳ Wait for the news section
            var newsSectionSelector = "section[data-testid='quoteNewsStream-0-Stream']";
            await page.WaitForSelectorAsync(newsSectionSelector, new() { Timeout = 5000 });
            
            var html = await page.ContentAsync();
            await browser.CloseAsync();

            //return new ContentResult
            //{
            //    Content = innerHtml,
            //    ContentType = "text/html",
            //};

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(innerHtml);

            _logger.LogInformation("Scraping Yahoo news for {Symbol}", symbol);

            var newsNodes = htmlDocument.DocumentNode.SelectNodes("//section[@data-testid='storyitem']");

            if (newsNodes == null)
            {
                return null;
            }

            var newsList = new List<YahooNews>();

            foreach (var node in newsNodes)
            {
                try
                {
                    var titleNode = node.SelectSingleNode(".//h3");
                    var urlNode = node.SelectSingleNode(".//a[@class='subtle-link']");
                    var summaryNode = node.SelectSingleNode(".//p");
                    var timeNode = node.SelectSingleNode(".//div[contains(@class, 'publishing')]");

                    string title = titleNode?.InnerText.Trim() ?? "N/A";
                    string newsUrl = urlNode?.GetAttributeValue("href", "").Trim() ?? "N/A";
                    if (!newsUrl.StartsWith("https")) newsUrl = "https://finance.yahoo.com" + newsUrl;
                    string summary = summaryNode?.InnerText.Trim() ?? "N/A";
                    string publishTime = timeNode?.InnerText.Trim() ?? "N/A";

                    newsList.Add(new YahooNews
                    {
                        Title = title,
                        Url = newsUrl,
                        Summary = summary,
                        PublishTime = publishTime
                    });
                }
                catch (Exception)
                {
                }

                if (newsList.Count >= limit)
                {
                    break;
                }
            }

            return JsonResult(newsList);
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
