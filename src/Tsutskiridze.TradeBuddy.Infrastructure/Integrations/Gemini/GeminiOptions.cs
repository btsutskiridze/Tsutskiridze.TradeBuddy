namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Gemini
{
    public class GeminiOptions
    {
        public const string SectionName = "AI:Gemini";
        public string ApiKey { get; set; } = string.Empty;
        public string ModelID { get; set; } = string.Empty;
    }
}

