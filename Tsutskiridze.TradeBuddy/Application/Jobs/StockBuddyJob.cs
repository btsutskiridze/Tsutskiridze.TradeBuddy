using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo;



//using Tsutskiridze.TradeBuddy.Helpers;

namespace Tsutskiridze.TradeBuddy.Application.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly StockAnalysisService _stockAnalysis;
        private readonly YahooStockScraper _yahooSraper;
        public StockBuddyJob(StockAnalysisService stockAnalysis, YahooStockScraper yahooSraper)
        {
            _yahooSraper = yahooSraper;
            _stockAnalysis = stockAnalysis;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //await _stockAnalysis.ExecuteStockAnalysisMultiple(Stocks.Favorites, TimeSpan.FromMinutes(5));
            //var quote = await _yahooSraper.GetStockQuote("AAPL");
            //var overview = await _yahooSraper.GetStockOverview("AAPL");
            //var anual = await _yahooSraper.GetStockLastAnnualReport("AAPL");
            //var prevdays = await _yahooSraper.GetStockPrevDaysClosePrices("AAPL", 5);

            //Console.WriteLine(JsonConvert.SerializeObject(quote, Formatting.Indented));
            //Console.WriteLine(JsonConvert.SerializeObject(overview, Formatting.Indented));
            //Console.WriteLine(JsonConvert.SerializeObject(anual, Formatting.Indented));
            //Console.WriteLine(JsonConvert.SerializeObject(prevdays, Formatting.Indented));

        }
    }
}
