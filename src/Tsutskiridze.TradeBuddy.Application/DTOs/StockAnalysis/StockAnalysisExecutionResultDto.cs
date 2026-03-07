namespace Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis
{
    public class StockAnalysisExecutionResultDto
    {
        public string Stock { get; set; }
        public StockAnalysisResultDto? Analysis { get; set; }
        public string? TelegramMessage { get; set; }
    }
}
