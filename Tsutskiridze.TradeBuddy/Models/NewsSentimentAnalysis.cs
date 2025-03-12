using System.ComponentModel.DataAnnotations;

namespace Tsutskiridze.TradeBuddy.Models
{
    public class NewsSentimentAnalysis
    {
        [Required]
        public string Google { get; set; }
        [Required]
        public string Reddit { get; set; }
        [Required]
        public string Yahoo { get; set; }
        [Required]
        public string Finnhub { get; set; }
    }
}
