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
            // set timer to calculate time taken for the operation
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true // Change to false for debugging
            });

            var searcUrl = $"https://www.google.com/search?q={symbol}&tbm=nws&tbs=sbd:1&hl=en";

            Console.WriteLine("Launching browser...");
            Console.WriteLine("searching for: " + searcUrl);

            var page = await browser.NewPageAsync();
            await page.GotoAsync(searcUrl,
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            Console.WriteLine("Navigating to Google News...");
            // Wait for news articles to load
            await page.WaitForSelectorAsync("div.SoaBEf");

            Console.WriteLine("Scraping news articles...");

            var newsList = new List<GoogleNews>();

            var newsItems = await page.QuerySelectorAllAsync("div.SoaBEf");
            foreach (var item in newsItems)
            {
                var titleDiv = await item.QuerySelectorAsync("div[class='n0jPhd ynAwRc MBeuO nDgy9d']");
                var urlAnchor = await item.QuerySelectorAsync("a");
                var summaryDiv = await item.QuerySelectorAsync("div[class='GI74Re nDgy9d']");
                var publishTimeDiv = await item.QuerySelectorAsync("div[class='OSrXXb rbYSKb LfVVr']");

                if (titleDiv == null || urlAnchor == null || summaryDiv == null || publishTimeDiv == null)
                {
                    Console.WriteLine("Failed to scrape news article.");
                    continue;
                }

                var title = await titleDiv.InnerTextAsync() ?? "N/A";
                var url = await urlAnchor.GetAttributeAsync("href") ?? "N/A";
                var summary = await summaryDiv.InnerTextAsync() ?? "N/A";
                var publishTime = await publishTimeDiv.InnerTextAsync() ?? "N/A";

                // Google URLs start with "/url?q="; fix this
                if (url.StartsWith("/url?q="))
                    url = url.Substring(7).Split("&")[0];

                newsList.Add(new GoogleNews
                {
                    Title = title,
                    Url = url,
                    Summary = summary,
                    PublishTime = ParsePublishTime(publishTime).ToString("yyyy-MM-ddTHH:mm:ssZ")
                });

                Console.WriteLine();
                Console.WriteLine($"Title: {title}");
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Summary: {summary}");
                Console.WriteLine($"Publish Time: {ParsePublishTime(publishTime):yyyy-MM-ddTHH:mm:ssZ}");
                Console.WriteLine();
            }

            watch.Stop();

            Console.WriteLine($"Time taken: {watch.Elapsed.TotalSeconds} s");
            Console.WriteLine("Scraping complete.");

            return newsList;
        }
        
        public static DateTime ParsePublishTime(string publishTime)
        {
            // Check if it's a relative time string (contains "ago")
            if (publishTime.ToLowerInvariant().Contains("ago"))
            {
                // Split the string into parts (e.g., "12 hours ago" becomes ["12", "hours", "ago"])
                var parts = publishTime.Split(' ');
                if (parts.Length >= 3 && int.TryParse(parts[0], out int value))
                {
                    DateTime now = DateTime.Now;
                    string unit = parts[1].ToLowerInvariant();

                    if (unit.StartsWith("hour"))
                        return now.AddHours(-value);
                    else if (unit.StartsWith("day"))
                        return now.AddDays(-value);
                    else if (unit.StartsWith("week"))
                        return now.AddDays(-7 * value);
                    else if (unit.StartsWith("minute"))
                        return now.AddMinutes(-value);
                    // Add additional units if needed.
                }
            }
            else
            {
                // Try parsing as an absolute date. For complex formats, you might need to specify a custom format.
                // For example, for "March 10, 2025 at 02:44 pm EDT", you might do:
                // "MMMM dd, yyyy 'at' hh:mm tt zzz" but note that "EDT" is not directly parseable by DateTime.Parse.
                // You may need to remove or map the timezone abbreviation first.
                if (DateTime.TryParse(publishTime, out DateTime absoluteDate))
                    return absoluteDate;
            }

            // Fallback: return MinValue if parsing fails.
            return DateTime.MinValue;
        }
    }
}
