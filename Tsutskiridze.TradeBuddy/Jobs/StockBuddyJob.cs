using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Services;
using Tsutskiridze.TradeBuddy.Services.News;

namespace Tsutskiridze.TradeBuddy.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly StockAnalysisService _stockAnalysis;
        private readonly GoogleScraper _googleScraper;
        public StockBuddyJob(StockAnalysisService stockAnalysis, GoogleScraper googleScraper)
        {
            _stockAnalysis = stockAnalysis;
            _googleScraper = googleScraper;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //await _googleScraper.GetNewsAsync("PLTR", 5);

            //    var result = await _stockAnalysis.ExecuteStockAnalysis("PLTR");

            //    if (result == null)
            //    {
            //        Console.WriteLine("Stock analysis returned null");
            //        return;
            //    }

            //    Console.WriteLine("==========================");
            //    Console.WriteLine(result.TelegramMessage);
            //    Console.WriteLine("==========================");

            //    Console.WriteLine("==========================");
            //    Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            //    Console.WriteLine("==========================");
        }
    }
}
