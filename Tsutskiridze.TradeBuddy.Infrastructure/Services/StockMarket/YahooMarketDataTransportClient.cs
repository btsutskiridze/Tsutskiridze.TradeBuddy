using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using Tsutskiridze.TradeBuddy.Application.Interfaces.StockMarket;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Services.StockMarket
{
    public class YahooMarketDataTransportClient : IMarketDataTransportClient
    {
        private readonly ILogger<YahooMarketDataTransportClient> _log;
        private readonly ClientWebSocket _ws = new();

        public YahooMarketDataTransportClient(ILogger<YahooMarketDataTransportClient> log)
            => _log = log;

        public async Task ConnectAsync(CancellationToken ct)
        {
            await _ws.ConnectAsync(new Uri("wss://streamer.finance.yahoo.com/?version=2"), ct);
            _log.LogInformation("Connected to Yahoo WS");
        }

        public async Task SendAsync(string message, CancellationToken ct)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
        }

        public async IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct)
        {
            var buffer = new byte[8192];
            var seg = new ArraySegment<byte>(buffer);

            using var ms = new MemoryStream();

            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                ms.SetLength(0);
                WebSocketReceiveResult res;
                do
                {
                    res = await _ws.ReceiveAsync(seg, ct);
                    if (res.MessageType == WebSocketMessageType.Close) yield break;
                    ms.Write(buffer, 0, res.Count);
                } while (!res.EndOfMessage);

                yield return Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
            }
        }

        public ValueTask DisposeAsync()
        {
            if (_ws.State != WebSocketState.Open) return ValueTask.CompletedTask;

            return new(_ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None));
        }
    }
}
