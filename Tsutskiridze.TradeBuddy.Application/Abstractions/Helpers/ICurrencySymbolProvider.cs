namespace Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers
{
    public interface ICurrencySymbolProvider
    {
        string? GetSymbol(string currencyCode);
    }
}
