using Newtonsoft.Json;
using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Services.News;
using Tsutskiridze.TradeBuddy.Services.Stocks;

namespace Tsutskiridze.TradeBuddy.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly AlphaVantageService _alphaVantage;
        private readonly FMPService _fmp;
        private readonly RedditService _reddit;
        private readonly FinnhubService _finnhub;
        private readonly YahooSraper _yahooSraper;
        private readonly GoogleScraper _googleScraper;

        public StockBuddyJob(AlphaVantageService alphaVantage, FMPService fmp, RedditService reddit, FinnhubService finnhub, YahooSraper yahooSraper, GoogleScraper googleScraper)
        {
            _alphaVantage = alphaVantage;
            _fmp = fmp;
            _reddit = reddit;
            _finnhub = finnhub;
            _yahooSraper = yahooSraper;
            _googleScraper = googleScraper;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string symbol = "RCAT";

            //var quote = await _fmp.GetStockQuote(symbol);
            //var stockOverview = await _alphaVantage.GetStockOverviewAsync(symbol);
            //var prevDayPrices = await _alphaVantage.GetStockPrevDaysClosePrices(symbol, 10);
            //var annualReport = await _alphaVantage.GetStockLastAnnualReport(symbol);
            //var redditNews = await _reddit.GetTopPostsAsync(symbol, "new", 3);
            //var finnhubNews = await _finnhub.GetCompanyNewsAsync(symbol, DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"), 3);
            //var yahooNews = await _yahooSraper.GetNewsAsync(symbol);

            var googleNews = await _googleScraper.GetNewsAsync(symbol);

            Console.WriteLine(JsonConvert.SerializeObject(googleNews, Formatting.Indented));

            //if (quote == null || stockOverview == null || prevDayPrices == null || annualReport == null)
            //{
            //    return;
            //}

            //var stock = new Stock
            //{
            //    Name = stockOverview.Name,
            //    Symbol = symbol,
            //    ReturnOnEquityTTM = stockOverview.ReturnOnEquityTTM,
            //    PriceToSalesRatioTTM = stockOverview.PriceToSalesRatioTTM,
            //    QuarterlyRevenueGrowthYOY = stockOverview.QuarterlyRevenueGrowthYOY,
            //    Quote = quote,
            //    PrevDays = prevDayPrices,
            //    AnnualReport = annualReport
            //};

            //Console.WriteLine(JsonConvert.SerializeObject(stock, Formatting.Indented));

        }
    }
}
