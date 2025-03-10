using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class RedditService
    {
        private readonly HttpClient _client;

        public RedditService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<RedditPost>?> GetTopPostsAsync(string keyword, string sortType, int? limit = null)
        {
            var response = await _client.GetAsync($"search.json?q={keyword}&sort={sortType}&type=posts");
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            if (json["data"]?["children"] == null)
            {
                return null;
            }

            return json["data"]["children"]
                .Where(post =>
                    post["data"]["score"].Value<int>() >= 5 && post["data"]["num_comments"].Value<int>() >= 5
                    && DateTimeOffset.FromUnixTimeSeconds(post["data"]["created_utc"].Value<long>()).DateTime > DateTime.Now.AddDays(-1)
                )
                .Take(limit ?? 5)
                .Select(post =>
                {
                    string body = post["data"]["selftext"].Value<string>();

                    return new RedditPost
                    {
                        Title = post["data"]["title"].Value<string>(),
                        Url = post["data"]["url"].Value<string>(),
                        Body = body.Length > 250 ? body.Substring(0, 250) + "..." : body,
                        Score = post["data"]["score"].Value<int>(),
                        CommentsCount = post["data"]["num_comments"].Value<int>(),
                        CreateTime = DateTimeOffset.FromUnixTimeSeconds(post["data"]["created_utc"].Value<long>()).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };
                })
                .ToList();
        }


    }
}
