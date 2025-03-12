using System.ComponentModel.DataAnnotations;

namespace Tsutskiridze.TradeBuddy.Models
{
    public class StockAnalysis
    {
        [Required]
        public string CurrentPrice { get; set; }
        [Required]
        public NewsSentimentAnalysis NewsSentimentAnalysis { get; set; }
        [Required]
        public string AiAnalysis { get; set; }
        [Required]
        public List<string> Reasoning { get; set; }
    }
}
