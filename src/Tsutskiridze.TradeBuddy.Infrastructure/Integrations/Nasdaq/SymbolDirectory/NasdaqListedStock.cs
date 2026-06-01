namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq.SymbolDirectory;

public sealed record NasdaqListedStock(
    string Symbol,
    string SecurityName);
