namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

public sealed record DailyStrategyCandle(
    DateOnly Date,
    decimal Close,
    decimal Low,
    decimal? Ema10,
    decimal? Ema20,
    decimal? Atr14,
    decimal? AdxToday,
    decimal? AdxYesterday,
    decimal? AdxTwoDaysAgo,
    decimal? AdxThreeDaysAgo);