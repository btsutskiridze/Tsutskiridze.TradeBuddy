using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Models;
using Tsutskiridze.TradeBuddy.Services.News;
using Tsutskiridze.TradeBuddy.Services.Stocks;

namespace Tsutskiridze.TradeBuddy.Services
{
    public class StockPromptService
    {
        private readonly AlphaVantageService _alphaVantage;
        private readonly FMPService _fmp;
        private readonly NewsService _news;
        public StockPromptService(AlphaVantageService alphaVantage, FMPService fmp, NewsService news)
        {
            _alphaVantage = alphaVantage;
            _fmp = fmp;
            _news = news;
        }

        public async Task<StockPrompt> GetStockPrompt(string symbol)
        {
            var quote = _fmp.GetStockQuote(symbol);
            var stockOverview = _alphaVantage.GetStockOverview(symbol);
            var prevDayPrices = _alphaVantage.GetStockPrevDaysClosePrices(symbol, 10);
            var annualReport = _alphaVantage.GetStockLastAnnualReport(symbol);
            var allNews = _news.GetAllNews(symbol, 2);

            await Task.WhenAll(quote, stockOverview, prevDayPrices, annualReport, allNews);

            if (quote.Result == null || stockOverview.Result == null || prevDayPrices.Result == null || annualReport.Result == null)
            {
                Console.WriteLine($"Failed to fetch stock data for {symbol}");
                Console.WriteLine($"Quote: {quote.Result}");
                Console.WriteLine($"StockOverview: {stockOverview.Result}");
                Console.WriteLine($"PrevDayPrices: {prevDayPrices.Result}");
                Console.WriteLine($"AnnualReport: {annualReport.Result}");
                Console.WriteLine($"AllNews: {allNews.Result}");

                //todo: fix alphavantage service

                throw new Exception("Failed to fetch stock data");
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

            return prompt;
        }


    }
}
