namespace Tsutskiridze.TradeBuddy.API.Contracts.Stocks;

public class GetFinnhubStockNewsRequest
{
    public DateTime From { get; init; }

    public DateTime To { get; init; }

    public int Limit { get; init; } = 10;
}