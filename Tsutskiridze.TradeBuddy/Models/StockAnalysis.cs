namespace Tsutskiridze.TradeBuddy.Models
{
    public class StockAnalysis
    {
        public string CurrentPrice { get; set; }
        public NewsSentimentAnalysis NewsSentimentAnalysis { get; set; }
        public string AiAnalysis { get; set; }
        public List<string> Reasoning { get; set; }
    }
}
