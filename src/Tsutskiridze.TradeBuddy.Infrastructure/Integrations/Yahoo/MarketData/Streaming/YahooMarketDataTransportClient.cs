using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Infrastructure.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming
{
    public sealed class YahooMarketDataTransportClient : IMarketDataTransportClient
    {
        private readonly ILogger<YahooMarketDataTransportClient> _log;
        private ClientWebSocket? _ws;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public YahooMarketDataTransportClient(ILogger<YahooMarketDataTransportClient> log)
            => _log = log;

        public async Task ConnectAsync(CancellationToken ct)
        {
            if (_ws?.State is WebSocketState.Open or WebSocketState.Connecting)
                return; // already connected/connecting

            _ws?.Dispose();
            _ws = new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(5) }
            };

            await _ws.ConnectAsync(new Uri("wss://streamer.finance.yahoo.com/?version=2"), ct);
            _log.LogInformation("Connected to Yahoo WS");
        }

        public async Task SendAsync(string message, CancellationToken ct)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            if (_ws is null)
                throw new InfrastructureException("WebSocket is not connected.");
            
            await _lock.WaitAsync(ct);
            try
            {
                await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct)
        {
            var buffer = new byte[8192];
            var seg = new ArraySegment<byte>(buffer);

            using var ms = new MemoryStream();

            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                ms.SetLength(0);
                WebSocketReceiveResult res;
                do
                {
                    res = await _ws.ReceiveAsync(seg, ct);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseOutputAsync(
                            WebSocketCloseStatus.NormalClosure, "Bye", ct);
                        yield break;                     
                    }

                    ms.Write(buffer, 0, res.Count);
                } while (!res.EndOfMessage);

                yield return Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
            }
        }

        public ValueTask DisposeAsync()
        {
            if (_ws is null) return ValueTask.CompletedTask;
            try
            {
                if (_ws.State == WebSocketState.Open)
                    return new ValueTask(_ws.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None));
            }
            finally
            {
                _ws.Dispose();
                _ws = null;
            }
            return ValueTask.CompletedTask;
        }
    }
}

