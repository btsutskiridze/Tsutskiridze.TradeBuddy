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

        public async Task<StockPrompt?> GetStockPrompt(string symbol)
        {
            try
            {
                var quote = _fmp.GetStockQuote(symbol);
                var stockOverview = _alphaVantage.GetStockOverview(symbol);
                var prevDayPrices = _alphaVantage.GetStockPrevDaysClosePrices(symbol, 10);
                var annualReport = _alphaVantage.GetStockLastAnnualReport(symbol);
                var allNews = _news.GetAllNews(symbol, 3);

                await Task.WhenAll(quote, stockOverview, prevDayPrices, annualReport, allNews);

                var stock = new StockDetails
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
                    Stock = stock
                };

                return prompt;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get stock prompt", ex);
            }
        }
    }
}
