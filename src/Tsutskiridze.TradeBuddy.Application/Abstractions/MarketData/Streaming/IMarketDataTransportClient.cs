namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming
{
    public interface IMarketDataTransportClient : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken ct);
        Task SendAsync(string message, CancellationToken ct);
        IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct);
    }
}
