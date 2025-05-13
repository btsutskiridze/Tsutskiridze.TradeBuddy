using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Core.Entities
{
    public class StockAnalysisModels
    {
        public class StockAnalysis
        {
            [Required]
            public string Symbol { get; set; }
            [Required]
            [Description("Current stock price, e.g. $4.43")]
            public string price { get; set; }

            [Required]
            [Description("50-day avg and 52-week high prices")]
            public BenchmarkMetrics bench { get; set; }

            [Required]
            [Description("Volume vs 50-day avg (50-60 chars)")]
            public string volAnalysis { get; set; }

            [Required]
            [Description("Sentiment by news source")]
            public List<SentimentDetail> news { get; set; }

            [Required]
            [Description("AI recommendation and confidence")]
            public AiAnalysis ai { get; set; }

            [Required]
            [Description("Key reasons (60-70 chars each)")]
            public List<string> reason { get; set; }

            [Required]
            [Description("Aggregate news sentiment")]
            public OveralNewsAnalysis newsOverall { get; set; }

            [JsonIgnore]
            public double ExecutionTime { get; set; }
        }

        public class BenchmarkMetrics
        {
            [Required]
            [Description("50-day avg price, e.g. $4.43")]
            public string avg50 { get; set; }

            [Required]
            [Description("52-week high price, e.g. \"$4.43\"")]
            public string yearHigh { get; set; }
        }

        public class SentimentDetail
        {
            [Required]
            [Description("Source, e.g. \"Google\"")]
            public string src { get; set; }

            [Required]
            [Description("Positive | Negative | Neutral")]
            public string sent { get; set; }

            [Required]
            [Description("Confidence, e.g. '70%'")]
            public string conf { get; set; }

            [Required]
            [Description("50-60-char explanation")]
            public string exp { get; set; }
        }

        public class AiAnalysis
        {
            [Required]
            [Description("BUY | SELL | HOLD")]
            public string rec { get; set; }

            [Required]
            [Description("Confidence, e.g. '65%'")]
            public string conf { get; set; }

            [Required]
            [Description("50-60-char explanation")]
            public string exp { get; set; }
        }

        public class OveralNewsAnalysis
        {
            [Required]
            [Description("Overall sentiment")]
            public string sent { get; set; }

            [Required]
            [Description("Overall confidence, e.g. \"70%\"")]
            public string conf { get; set; }

            [Required]
            [Description("60-70-char explanation")]
            public string exp { get; set; }
        }

    }
}


