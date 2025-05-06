using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace Tsutskiridze.TradeBuddy.Application.Jobs
{
    public class TestJob : BackgroundService
    {

        private const string WSS_URL = "wss://streamer.finance.yahoo.com/?version=2";
        private ClientWebSocket _ws = null!;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TestJob> _logger;

        public TestJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _ws = new ClientWebSocket();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _ws.ConnectAsync(new Uri(WSS_URL), stoppingToken);

            var symbols = new string[] { "PLRZ", "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "NFLX", "META", "NVDA", "AMD", "INTC", "CSCO", "IBM", "ORCL", "ADBE", "CRM" };

            var subscribePayload = JsonConvert.SerializeObject(new { subscribe = symbols });

            await Subscribe(subscribePayload, stoppingToken);

            var buffer = new byte[8192];
            while (!stoppingToken.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(buffer, stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"Server closed connection ({result.CloseStatus}): {result.CloseStatusDescription}");
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    JObject keyValuePairs = JObject.Parse(msg);
                    var data = Convert.FromBase64String(keyValuePairs["message"].ToString()!);

                    Console.WriteLine($"Received message: {JsonConvert.SerializeObject(keyValuePairs, Formatting.Indented)}");

                    try
                    {

                        //if (update.Id == "PLRZ)
                        //{
                        //Console.WriteLine($"{update.Id} = {update.Price}({update.Change} - {update.ChangePercent}%)");
                        //Console.WriteLine(JsonConvert.SerializeObject(update, Formatting.Indented));
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("   ⚠ Protobuf parse error: " + ex.Message);
                    }

                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {

                }
            }

            Console.WriteLine("Listener stopping.");
        }

        private Task Subscribe(string text, CancellationToken ct)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
        }
    }

}
