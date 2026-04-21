using System.Globalization;

namespace Tsutskiridze.TradeBuddy.Application.Common.Localization
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

