using Tsutskiridze.TradeBuddy.Application.Contracts.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Contracts.Providers.MarketData;
using Tsutskiridze.TradeBuddy.Application.Features.NewsAggregation;
using Tsutskiridze.TradeBuddy.Core.Constants;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis
{
    public class StockPromptService
    {
        private readonly NewsService _news;
        private readonly IYahooMarketDataProvider _yahooSraper;

        public StockPromptService(NewsService news, IYahooMarketDataProvider yahooSraper)
        {
            _news = news;
            _yahooSraper = yahooSraper;
        }

        public async Task<StockAnalysisPromptDto?> GetStockPrompt(string symbol)
        {
            try
            {
                var quote = _yahooSraper.GetStockQuote(symbol);
                var stockOverview = _yahooSraper.GetStockOverview(symbol);
                var prevDayPrices = _yahooSraper.GetStockPrevDaysClosePrices(symbol, 10);
                var annualReport = _yahooSraper.GetStockLastAnnualReport(symbol);
                var allNews = _news.GetAllNews(symbol, 3);

                await Task.WhenAll(quote, stockOverview, prevDayPrices, annualReport, allNews);

                var stock = new StockAnalysisContextDto
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

                var prompt = new StockAnalysisPromptDto
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
