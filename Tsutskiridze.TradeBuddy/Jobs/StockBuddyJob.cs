using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
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

        public StockBuddyJob(AlphaVantageService alphaVantage)
        {
            _alphaVantage = alphaVantage;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //var prices = await _alphaVantage.GetStockPrevDaysClosePrices("RCAT", 5);
            //var annualReport = await _alphaVantage.GetStockLastAnnualReport("RCAT");
        }
    }
}
