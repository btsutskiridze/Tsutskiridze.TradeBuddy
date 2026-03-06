namespace Tsutskiridze.TradeBuddy.Application.Contracts.Services.Helpers
{
    public interface ICurrencySymbolProvider
    {
        string? GetSymbol(string currencyCode);
    }
}
