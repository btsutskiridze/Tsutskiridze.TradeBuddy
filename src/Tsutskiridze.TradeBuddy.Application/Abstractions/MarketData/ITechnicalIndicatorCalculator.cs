using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;

public interface ITechnicalIndicatorCalculator
{
    IReadOnlyList<DailyStrategyCandle> BuildDailyStrategyCandles(
        IReadOnlyList<MarketCandle> candles);
}