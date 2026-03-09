using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.FinancialModelingPrep.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Mapping
{
    public static class MarketDataMappingExtensions
    {
        public static AnnualReportDto? ToDto(this AlphaVantageAnnualReportResponse? source)
        {
            if (source is null ) return null;

            return new AnnualReportDto
            {
                FiscalDateEnding = source.FiscalDateEnding,
                ReportedCurrency = source.ReportedCurrency,
                TotalRevenue = source.TotalRevenue,
                CostOfRevenue = source.CostOfRevenue,
                GrossProfit = source.GrossProfit,
                OperatingIncome = source.OperatingIncome,
                NetIncome = source.NetIncome,
                DepreciationAndAmortization = source.DepreciationAndAmortization
            };
        }

        public static StockOverviewDto? ToDto(this AlphaVantageStockOverviewResponse? source)
        {
            if (source is null ) return null;

            return new StockOverviewDto
            {
                ReturnOnEquityTTM = source.ReturnOnEquityTTM,
                PriceToSalesRatioTTM = source.PriceToSalesRatioTTM,
                QuarterlyRevenueGrowthYOY = source.QuarterlyRevenueGrowthYOY,
                PriceAvg50 = source.PriceAvg50,
                PriceAvg200 = source.PriceAvg200,
                SharesOutstanding = source.SharesOutstanding
            };
        }

        public static StockQuoteDto ToDto(this FmpStockQuoteResponse source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return new StockQuoteDto
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
}
