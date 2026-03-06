namespace Tsutskiridze.TradeBuddy.Application.Contracts.News
{
    public class RedditPostDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public int Score { get; set; }
        public int CommentsCount { get; set; }
        public string CreateTime { get; set; }
    }
}
