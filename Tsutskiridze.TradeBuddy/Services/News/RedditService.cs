using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;
using System.Text;
using System.Text.Json;

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

            var posts = new List<RedditPost>();

            foreach (var item in json["data"]["children"])
            {
                try
                {
                    if (item["data"]["score"].Value<int>() >= 5 && item["data"]["num_comments"].Value<int>() >= 5)
                    {
                        string body = item["data"]["selftext"].Value<string>();
                        posts.Add(new RedditPost
                        {
                            Title = item["data"]["title"].Value<string>(),
                            Url = item["data"]["url"].Value<string>(),
                            Body = body.Length > 250 ? body.Substring(0, 250) + "..." : body,
                            Score = item["data"]["score"].Value<int>(),
                            CommentsCount = item["data"]["num_comments"].Value<int>(),
                            CreateTime = DateTimeOffset.FromUnixTimeSeconds(item["data"]["created_utc"].Value<long>()).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
                catch (Exception)
                {
                }

                if (posts.Count >= limit)
                {
                    break;
                }
            }

            return posts.Count > 0 ? posts : null;

        }


    }
}
