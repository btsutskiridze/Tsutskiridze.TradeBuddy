namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

public sealed record MarketCandle(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume
);