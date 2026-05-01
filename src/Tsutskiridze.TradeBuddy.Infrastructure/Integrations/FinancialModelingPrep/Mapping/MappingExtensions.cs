using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep.Mapping;

public static class MappingExtensions
{
    public static StockQuote ToDto(this FmpStockQuoteResponse source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        return new StockQuote
        {
            Symbol = source.Symbol,
            Name = source.Name,
            // Currency is not provided by FmpStockQuoteResponse
            Currency = string.Empty,
            Price = source.Price.ToString(),
            ChangesPercentage = source.ChangesPercentage.ToString(),
            Change = source.Change.ToString(),
            DayLow = source.DayLow.ToString(),
            DayHigh = source.DayHigh.ToString(),
            YearHigh = source.YearHigh.ToString(),
            YearLow = source.YearLow.ToString(),
            MarketCap = source.MarketCap.ToString(),
            Exchange = source.Exchange,
            Volume = source.Volume.ToString(),
            AvgVolume = source.AvgVolume.ToString(),
            Open = source.Open.ToString(),
            PreviousClose = source.PreviousClose.ToString(),
            Eps = source.Eps.ToString(),
            Pe = source.Pe?.ToString(),
            EarningsAnnouncement = source.EarningsAnnouncement.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Timestamp = DateTimeOffset
                .FromUnixTimeSeconds(long.Parse(source.Timestamp))
                .UtcDateTime
                .ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}