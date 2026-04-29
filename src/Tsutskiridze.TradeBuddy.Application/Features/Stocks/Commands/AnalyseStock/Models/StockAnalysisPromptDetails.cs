using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Models
{
    public class StockAnalysisPromptDetails : StockOverview
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public StockQuote? Quote { get; set; }
        public List<StockDayPrice>? PrevDays { get; set; }
        public AnnualReport? AnnualReport { get; set; }
        public StockNews? News { get; set; }
    }
}
