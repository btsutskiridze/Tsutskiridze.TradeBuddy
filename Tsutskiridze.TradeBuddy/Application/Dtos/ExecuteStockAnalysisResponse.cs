using Tsutskiridze.TradeBuddy.Models;

namespace Tsutskiridze.TradeBuddy.Application.Dtos
{
    public class ExecuteStockAnalysisResponse
    {
        public string Stock { get; set; }
        public StockAnalysisModels.StockAnalysis? Analysis { get; set; }
        public string? TelegramMessage { get; set; }
    }
}
