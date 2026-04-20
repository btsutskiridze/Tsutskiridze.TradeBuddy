using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Prompts;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Services
{
    public class StockAnalysisPromptBuilder
    {
        private readonly INewsProvider _news;
        private readonly IMarketDataProvider _scraper;

        public StockAnalysisPromptBuilder(INewsProvider news, IMarketDataProvider scraper)
        {
            _news = news;
            _scraper = scraper;
        }

        public async Task<StockAnalysisPromptPayloadDto> BuildAsync(string symbol)
        {
            try
            {
                var quote = _scraper.GetStockQuote(symbol);
                var stockOverview = _scraper.GetStockOverview(symbol);
                var prevDayPrices = _scraper.GetStockPrevDaysClosePrices(symbol, 10);
                var annualReport = _scraper.GetStockLastAnnualReport(symbol);
                var allNews = _news.GetAllNews(symbol, 3);

                await Task.WhenAll(quote, stockOverview, prevDayPrices, annualReport, allNews);

                var stock = new StockAnalysisInputDto
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

                return new StockAnalysisPromptPayloadDto
                {
                    AnalysisRequest = StockAnalysisPromptTemplate.AnalysisRequest,
                    InvestmentHorizon = StockAnalysisPromptTemplate.InvestmentHorizon,
                    Stock = stock
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to build stock analysis prompt", ex);
            }
        }
    }
}
