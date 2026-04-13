namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Streaming.Yahoo.Contracts
{
    public interface IMarketDataTransportClient : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken ct);
        Task SendAsync(string message, CancellationToken ct);
        IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct);
    }
}
