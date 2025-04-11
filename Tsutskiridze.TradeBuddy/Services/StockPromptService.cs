using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Models;
using Tsutskiridze.TradeBuddy.Services.News;
using Tsutskiridze.TradeBuddy.Services.Stocks;
using Tsutskiridze.TradeBuddy.Services.Yahoo;

namespace Tsutskiridze.TradeBuddy.Services
{
    public class StockPromptService
    {
        private readonly FMPService _fmp;
        private readonly NewsService _news;
        private readonly YahooSraper _yahooSraper;

        public StockPromptService(FMPService fmp, NewsService news, YahooSraper yahooSraper)
        {
            _fmp = fmp;
            _news = news;
            _yahooSraper = yahooSraper;
        }

        public async Task<StockPrompt?> GetStockPrompt(string symbol)
        {
            try
            {
                var quote = _yahooSraper.GetStockQuote(symbol);
                var stockOverview = _yahooSraper.GetStockOverview(symbol);
                var prevDayPrices = _yahooSraper.GetStockPrevDaysClosePrices(symbol, 10);
                var annualReport = _yahooSraper.GetStockLastAnnualReport(symbol);
                var allNews = _news.GetAllNews(symbol, 3);

                await Task.WhenAll(quote, stockOverview, prevDayPrices, annualReport, allNews);

                var stock = new StockDetails
                {
                    Name = quote.Result.Name,
                    Symbol = symbol,
                    Currency = quote.Result.Currency,
                    ReturnOnEquityTTM = stockOverview.Result.ReturnOnEquityTTM,
                    PriceToSalesRatioTTM = stockOverview.Result.PriceToSalesRatioTTM,
                    QuarterlyRevenueGrowthYOY = stockOverview.Result.QuarterlyRevenueGrowthYOY,
                    PriceAvg50 = stockOverview.Result.PriceAvg50,
                    PriceAvg200 = stockOverview.Result.PriceAvg200,
                    SharesOutstanding = stockOverview.Result.SharesOutstanding,
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
