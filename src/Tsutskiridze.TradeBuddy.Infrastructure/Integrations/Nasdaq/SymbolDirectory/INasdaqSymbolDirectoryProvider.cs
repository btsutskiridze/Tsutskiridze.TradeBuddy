namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq.SymbolDirectory;

public interface INasdaqSymbolDirectoryProvider
{
    Task<NasdaqSymbolLookupResult> StockSymbolExists(
        string symbol,
        CancellationToken ct = default);

    Task<IReadOnlyList<NasdaqListedStock>> GetNasdaqListedStocks(
        CancellationToken ct = default);
}
