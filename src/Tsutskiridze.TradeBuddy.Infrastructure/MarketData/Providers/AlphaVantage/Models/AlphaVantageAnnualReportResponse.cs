namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage.Models
{
    public class AlphaVantageAnnualReportResponse
    {
        public string FiscalDateEnding { get; set; }
        public string ReportedCurrency { get; set; }
        public string TotalRevenue { get; set; }
        public string CostOfRevenue { get; set; }
        public string GrossProfit { get; set; }
        public string OperatingIncome { get; set; }
        public string NetIncome { get; set; }
        public string DepreciationAndAmortization { get; set; }
    }
}
