using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Models
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
            [Description("50-day avg and yearly high")]
            public BenchmarkMetrics bench { get; set; }

            [Required]
            [Description("Trading volume vs avg (50-60chars)")]
            public string volAnalysis { get; set; }

            [Required]
            [Description("Sentiment analysis from news")]
            public List<SentimentDetail> news { get; set; }

            [Required]
            [Description("AI rec and confidence")]
            public AiAnalysis ai { get; set; }

            [Required]
            [Description("Analysis reasons (60-70chars each)")]
            public List<string> reason { get; set; }

            [Required]
            [Description("Overall news sentiment")]
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
            [Description("Yearly high price, e.g. $4.43")]
            public string yearHigh { get; set; }
        }

        public class SentimentDetail
        {
            [Required]
            [Description("News source, e.g. Google")]
            public string src { get; set; }

            [Required]
            [Description("Sentiment (Positive, Negative, Neutral)")]
            public string sent { get; set; }

            [Required]
            [Description("Confidence, e.g. '70%'")]
            public string conf { get; set; }

            [Required]
            [Description("Short sentiment explanation (50-60chars)")]
            public string exp { get; set; }
        }

        public class AiAnalysis
        {
            [Required]
            [Description("Rec (HOLD, BUY, SELL)")]
            public string rec { get; set; }

            [Required]
            [Description("Confidence, e.g. '65%'")]
            public string conf { get; set; }

            [Required]
            [Description("Rec explanation (50-60chars)")]
            public string exp { get; set; }
        }

        public class OveralNewsAnalysis
        {
            [Required]
            [Description("Overall sentiment")]
            public string sent { get; set; }

            [Required]
            [Description("Overall confidence")]
            public string conf { get; set; }

            [Required]
            [Description("Overall explanation (60-70chars)")]
            public string exp { get; set; }
        }

    }
}


