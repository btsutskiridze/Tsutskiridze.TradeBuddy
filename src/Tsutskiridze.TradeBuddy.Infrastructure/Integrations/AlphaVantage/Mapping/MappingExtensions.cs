using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage.Mapping;

public static class MappingExtensions
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
   
}