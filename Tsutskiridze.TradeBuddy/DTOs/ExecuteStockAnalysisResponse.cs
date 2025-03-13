using Tsutskiridze.TradeBuddy.Models;

namespace Tsutskiridze.TradeBuddy.DTOs
{
    public class ExecuteStockAnalysisResponse
    {
        public StockAnalysisModels.StockAnalysis Analysis { get; set; }
        public string TelegramMessage { get; set; }
    }
}
