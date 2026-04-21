namespace Tsutskiridze.TradeBuddy.Application.Common.Localization
{
    //todo: refactor this abstractions too
    public interface ICurrencySymbolProvider
    {
        string? GetSymbol(string currencyCode);
    }
}
