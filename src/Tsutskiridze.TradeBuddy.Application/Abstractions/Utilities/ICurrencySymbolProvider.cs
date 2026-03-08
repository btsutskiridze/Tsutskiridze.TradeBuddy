namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities
{
    public interface ICurrencySymbolProvider
    {
        string? GetSymbol(string currencyCode);
    }
}
