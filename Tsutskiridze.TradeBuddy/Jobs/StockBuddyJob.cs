using Newtonsoft.Json;
using System.Diagnostics;
using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.DTOs.Helpers;
using Tsutskiridze.TradeBuddy.Models;
using Tsutskiridze.TradeBuddy.Services.News;
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
        private readonly NewsService _news;
        public StockBuddyJob(AlphaVantageService alphaVantage, FMPService fmp, NewsService news)
        {
            _alphaVantage = alphaVantage;
            _fmp = fmp;
            _news = news;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string symbol = "RCAT";

            var watch = Stopwatch.StartNew();

            var quote = _fmp.GetStockQuote(symbol);
            var stockOverview = _alphaVantage.GetStockOverview(symbol);
            var prevDayPrices = _alphaVantage.GetStockPrevDaysClosePrices(symbol, 10);
            var annualReport = _alphaVantage.GetStockLastAnnualReport(symbol);
            var allNews = _news.GetAllNews(symbol, 2);

            await Task.WhenAll(quote, stockOverview, prevDayPrices, annualReport, allNews);

            if (quote.Result == null || stockOverview.Result == null || prevDayPrices.Result == null || annualReport.Result == null)
            {
                return;
            }

            var stock = new Stock
            {
                Name = stockOverview.Result.Name,
                Symbol = symbol,
                ReturnOnEquityTTM = stockOverview.Result.ReturnOnEquityTTM,
                PriceToSalesRatioTTM = stockOverview.Result.PriceToSalesRatioTTM,
                QuarterlyRevenueGrowthYOY = stockOverview.Result.QuarterlyRevenueGrowthYOY,
                Quote = quote.Result,
                PrevDays = prevDayPrices.Result,
                AnnualReport = annualReport.Result,
                News = allNews.Result
            };

            var prompt = new StockPrompt
            {
                AnalysisRequest = StockPromptParams.AnalysisRequest,
                InvestmentHorizon = StockPromptParams.InvestmentHorizon,
                ResponseStructure = StockPromptParams.ResponseStructure,
                Stock = stock
            };

            Console.WriteLine(JsonConvert.SerializeObject(prompt, Formatting.Indented));

            watch.Stop();

            Console.WriteLine($"Execution Time: {watch.Elapsed.TotalSeconds} s");
        }
    }
}
