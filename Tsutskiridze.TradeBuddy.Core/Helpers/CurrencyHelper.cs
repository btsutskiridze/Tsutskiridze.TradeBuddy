using System.Globalization;

namespace Tsutskiridze.TradeBuddy.Core.Helpers
{
    public static class CurrencyHelper
    {
        public static string GetCurrencySymbol(string currencyCode)
        {
            try
            {
                var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .FirstOrDefault(c => new RegionInfo(c.Name).ISOCurrencySymbol == currencyCode);

                return culture != null ? new RegionInfo(culture.Name).CurrencySymbol : "Unknown Symbol";
            }
            catch
            {
                return "Invalid Code";
            }
        }
    }
}
