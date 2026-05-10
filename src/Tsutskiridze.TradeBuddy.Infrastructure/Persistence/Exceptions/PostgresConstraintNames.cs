namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;

internal static class PostgresConstraintNames
{
    public const string PriceAlertsUniqueBusinessKey =
        "UX_price_alerts_chat_stock_direction_price";

    public const string StocksSymbol =
        "UX_stocks_symbol";
}