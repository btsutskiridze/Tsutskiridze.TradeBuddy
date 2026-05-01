namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI
{
    public class OpenAiOptions
    {
        public const string SectionName = "AI:OpenAI";
        public string ApiKey { get; set; } = string.Empty;
        public string ModelID { get; set; } = string.Empty;
    }
}

