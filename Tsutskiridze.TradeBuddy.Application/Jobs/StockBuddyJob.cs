using Quotefeeder;
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
            await ConnectAndSubscribe(new string[] { "AAPL", "GOOG", "MSFT" });
        }
        public async Task ConnectAndSubscribe(string[] symbols)
        {
            await _ws.ConnectAsync(new Uri(WSS_URL), CancellationToken.None);

            // Example subscription message (Yahoo uses JSON for subscription)
            var subscribeMsg = $"{{\"subscribe\":[\"{string.Join("\",\"", symbols)}\"]}}";
            await SendWebSocketMessage(subscribeMsg);

            _ = Task.Run(StartListening);
        }

        private async Task StartListening()
        {
            var buffer = new byte[4096];
            Console.WriteLine("Listening for messages...");
            while (_ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var data = new byte[result.Count];
                    Array.Copy(buffer, data, result.Count);
                    // Process the binary data here
                    // For example, you can convert it to a string and print it
                    var message = System.Text.Encoding.UTF8.GetString(data);
                    Console.WriteLine($"Received: {message}");
                    // You can also deserialize the message if it's in JSON format
                    var res = PriceUpdate.Parser.ParseFrom(data);

                    Console.WriteLine($"Received: {res.PricingData.ShortName} => {res.PricingData.Price}");
                }
            }
            Console.WriteLine("WebSocket closed.");
        }

        private async Task SendWebSocketMessage(string message)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(new ArraySegment<byte>(bytes),
                              WebSocketMessageType.Text,
                              true,
                              CancellationToken.None);
        }
    }
}
