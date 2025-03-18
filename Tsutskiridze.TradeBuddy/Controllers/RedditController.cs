using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.Services.News;

namespace Tsutskiridze.TradeBuddy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedditController : ApiControllerBase
    {

        private readonly RedditService _reddit;
        private readonly ILogger<RedditController> _logger;

        public RedditController(RedditService reddit, ILogger<RedditController> logger)
        {
            _reddit = reddit;
            _logger = logger;
        }


        [HttpPost("stocks/{symbol}")]
        public async Task<IActionResult> GetRedditPosts(string symbol)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var searcUrl = $"https://www.reddit.com/search??q={symbol}&sort=new&type=posts";

            _logger.LogInformation("searchUrl ==> {searchUrl}", searcUrl);

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                            "(KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36"
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(searcUrl,
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            //try
            //{
            //    // Example: if there's a button with text "I agree" or "Accept all"
            //    var acceptAllButton = page.Locator("button[aria-label='Accept all']").First;
            //    if (await acceptAllButton.IsVisibleAsync())
            //    {
            //        await acceptAllButton.ClickAsync();
            //        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // If not found or something else fails, just log and continue
            //    _logger.LogError("Consent popup not found or click failed: {err}", ex.Message);
            //}


            // Log the page's HTML content (be aware this can be large)
            var pageContent = await page.ContentAsync();
            _logger.LogInformation("Page HTML content:\n{pageContent}", pageContent);


            return Ok(pageContent);
        }
    }
}
