using Microsoft.Playwright;
using Tsutskiridze.TradeBuddy.DTOs.GoogleNews;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class GoogleScraper
    {
        private readonly HttpClient _httpClient;

        public GoogleScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GoogleNews>> GetNewsAsync(string symbol)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true // Change to false for debugging
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync($"https://www.google.com/search?q={symbol}+stock&tbm=nws");

            // Wait for news articles to load
            await page.WaitForSelectorAsync("div.SoaBEf");

            var newsList = new List<GoogleNews>();

            var newsItems = await page.QuerySelectorAllAsync("div.SoaBEf");
            foreach (var item in newsItems)
            {
                var title = await (await item.QuerySelectorAsync("div.mCBkyc"))?.InnerTextAsync() ?? "N/A";
                var url = await (await item.QuerySelectorAsync("a"))?.GetAttributeAsync("href") ?? "N/A";
                var summary = await (await item.QuerySelectorAsync("div.GI74Re"))?.InnerTextAsync() ?? "N/A";
                var publishTime = await (await item.QuerySelectorAsync("div.OSrXXb"))?.InnerTextAsync() ?? "N/A";

                // Google URLs start with "/url?q="; fix this
                if (url.StartsWith("/url?q="))
                    url = url.Substring(7).Split("&")[0];

                newsList.Add(new GoogleNews
                {
                    Title = title,
                    Url = url,
                    Summary = summary,
                    PublishTime = publishTime
                });
            }

            return newsList;
        }
    }
}
