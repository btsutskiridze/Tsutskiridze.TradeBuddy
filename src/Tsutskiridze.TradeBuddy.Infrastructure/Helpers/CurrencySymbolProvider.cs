using System.Globalization;
using Tsutskiridze.TradeBuddy.Application.Contracts.Services.Helpers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Helpers
{
    public class CurrencySymbolProvider : ICurrencySymbolProvider
    {
        public string? GetSymbol(string currencyCode)
        {
            try
            {
                var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .FirstOrDefault(c => new RegionInfo(c.Name).ISOCurrencySymbol == currencyCode);

                return culture != null ? new RegionInfo(culture.Name).CurrencySymbol : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
