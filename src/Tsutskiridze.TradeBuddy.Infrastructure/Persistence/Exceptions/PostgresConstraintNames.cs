namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;

internal static class PostgresConstraintNames
{
    public const string PriceAlertsUniqueBusinessKey =
        "ux_price_alerts_chat_stock_direction_price";

    public const string StocksSymbol =
        "ux_stocks_symbol";
}