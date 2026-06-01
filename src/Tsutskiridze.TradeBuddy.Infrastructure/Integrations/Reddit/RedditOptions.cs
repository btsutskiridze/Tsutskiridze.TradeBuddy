namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit
{
    public class RedditOptions
    {
        public const string SectionName = "Reddit";
        public const string ResiliencePipelineName = SectionName;
        public string SearchEndpoint { get; set; } = string.Empty;
        public string AuthEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}

