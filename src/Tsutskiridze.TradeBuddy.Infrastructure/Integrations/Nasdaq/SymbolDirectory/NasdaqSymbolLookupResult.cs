namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq.SymbolDirectory;

public sealed record NasdaqSymbolLookupResult(bool IsSuccessful, bool Exists)
{
    public static NasdaqSymbolLookupResult Success(bool exists) => new(true, exists);

    public static NasdaqSymbolLookupResult Failure() => new(false, false);
}
