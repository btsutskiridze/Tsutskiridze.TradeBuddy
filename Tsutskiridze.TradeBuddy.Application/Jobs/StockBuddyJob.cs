using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Yahoo;

namespace Tsutskiridze.TradeBuddy.Application.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly StockAnalysisService _stockAnalysis;
        private readonly IYahooStockScraper _yahooSraper;
        public StockBuddyJob(StockAnalysisService stockAnalysis, IYahooStockScraper yahooSraper)
        {
            _yahooSraper = yahooSraper;
            _stockAnalysis = stockAnalysis;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //await _stockAnalysis.ExecuteStockAnalysisMultiple(Stocks.Favorites, TimeSpan.FromMinutes(5));
            //await _stockAnalysis.ExecuteStockAnalysis("MVST");

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
