using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Services;

namespace Tsutskiridze.TradeBuddy.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly StockAnalysisService _stockAnalysis;
        public StockBuddyJob(StockAnalysisService stockAnalysis)
        {
            _stockAnalysis = stockAnalysis;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //await _stockAnalysis.ExecuteStockAnalysisMultiple(Stocks.Favorites);
        }
    }
}
