using System.Net.WebSockets;
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
        // The Yahoo Finance streamer endpoint
        private const string WSS_URL = "wss://streamer.finance.yahoo.com/?version=2";
        private ClientWebSocket _ws = new();

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
        }
    }
}
