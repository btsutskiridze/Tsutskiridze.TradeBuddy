namespace Tsutskiridze.TradeBuddy.Application.Interfaces.StockMarket
{
    public interface IMarketDataTransportClient : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken ct);
        Task SendAsync(string message, CancellationToken ct);
        IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct);
    }
}
