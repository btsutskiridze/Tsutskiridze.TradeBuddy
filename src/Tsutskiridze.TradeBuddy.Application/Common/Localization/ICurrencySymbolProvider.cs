namespace Tsutskiridze.TradeBuddy.Application.Common.Localization
{
    public interface ICurrencySymbolProvider
    {
        string? GetSymbol(string currencyCode);
    }
}
