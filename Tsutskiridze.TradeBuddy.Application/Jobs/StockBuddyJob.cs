using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;
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
        private readonly IServiceProvider _provider;
        public StockBuddyJob(StockAnalysisService stockAnalysis, IYahooStockScraper yahooSraper, IServiceProvider serviceProvider)
        {
            _stockAnalysis = stockAnalysis;
            _yahooSraper = yahooSraper;
            _provider = serviceProvider;
        }
        // The Yahoo Finance streamer endpoint
        private const string WSS_URL = "wss://streamer.finance.yahoo.com/?version=2";
        private ClientWebSocket _ws = new();

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var getCommandHandlers = _provider.GetServices<ITelegramCommandHandler>();

            var alertcommandhandler = getCommandHandlers.FirstOrDefault(x => x.Command == "alert");

            if (alertcommandhandler == null)
            {
                throw new Exception("AlertCommandHandler not found");
            }

            await Task.Delay(3000, cancellationToken);

            foreach (var handler in getCommandHandlers)
            {
                await handler.HandleMessage(new Telegram.Bot.Types.Message
                {
                    Chat = new Telegram.Bot.Types.Chat
                    {
                        Id = -4659763511 // Replace with actual chat ID
                    },
                    Text = $"/{handler.Command}"
                });
            }
        }
    }
}
