namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Helpers
{
    public interface ICurrencySymbolProvider
    {
        string? GetSymbol(string currencyCode);
    }
}
