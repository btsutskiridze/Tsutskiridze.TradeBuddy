using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage.Mapping;

public static class MappingExtensions
{
    public static AnnualReport? ToDto(this AlphaVantageAnnualReportResponse? source)
    {
        if (source is null ) return null;

        return new AnnualReport
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

    public static StockOverview? ToDto(this AlphaVantageStockOverviewResponse? source)
    {
        if (source is null ) return null;

        return new StockOverview
        {
            ReturnOnEquityTTM = source.ReturnOnEquityTTM,
            PriceToSalesRatioTTM = source.PriceToSalesRatioTTM,
            QuarterlyRevenueGrowthYOY = source.QuarterlyRevenueGrowthYOY,
            PriceAvg50 = source.PriceAvg50,
            PriceAvg200 = source.PriceAvg200,
            SharesOutstanding = source.SharesOutstanding
        };
    }
   
}