using System.Globalization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.TextFormatting.Telegram;

public static class TelegramValueFormatter
{
    public static string Decimal(decimal value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    public static string NullableDecimal(decimal? value)
    {
        return value.HasValue
            ? Decimal(value.Value)
            : "n/a";
    }

    public static string Percent(decimal value)
    {
        return value.ToString("N2", CultureInfo.InvariantCulture) + "%";
    }
}
