namespace Tsutskiridze.TradeBuddy.Application.Abstractions.StockMarket
{
    public interface IMarketDataTransportClient : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken ct);
        Task SendAsync(string message, CancellationToken ct);
        IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct);
    }
}
