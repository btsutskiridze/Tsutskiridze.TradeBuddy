using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Services.Stocks;

namespace Tsutskiridze.TradeBuddy.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly AlphaVantageService _alphaVantage;
        private readonly FMPService _fmp;

        public StockBuddyJob(AlphaVantageService alphaVantage, FMPService fmp)
        {
            _alphaVantage = alphaVantage;
            _fmp = fmp;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string symbol = "RCAT";

            //var quote = await _fmp.GetStockQuote(symbol);
            //var stockOverview = await _alphaVantage.GetStockOverviewAsync(symbol);
            //var prevDayPrices = await _alphaVantage.GetStockPrevDaysClosePrices(symbol, 10);
            //var annualReport = await _alphaVantage.GetStockLastAnnualReport(symbol);

            //if (quote == null || stockOverview == null || prevDayPrices == null || annualReport == null)
            //{
            //    return;
            //}

            //var stock = new Stock
            //{
            //    Name = stockOverview.Name,
            //    Symbol = symbol,
            //    ReturnOnEquityTTM = stockOverview.ReturnOnEquityTTM,
            //    PriceToSalesRatioTTM = stockOverview.PriceToSalesRatioTTM,
            //    QuarterlyRevenueGrowthYOY = stockOverview.QuarterlyRevenueGrowthYOY,
            //    Quote = quote,
            //    PrevDays = prevDayPrices,
            //    AnnualReport = annualReport
            //};

            //Console.WriteLine(JsonConvert.SerializeObject(stock, Formatting.Indented));

        }
    }
}
