namespace Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI
{
    public class OpenAIOptions
    {
        public const string SectionName = "AI:OpenAI";
        public string ApiKey { get; set; } = string.Empty;
        public string ModelID { get; set; } = string.Empty;
    }
}
