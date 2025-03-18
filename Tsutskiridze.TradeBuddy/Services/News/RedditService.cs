using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class RedditService
    {
        private readonly HttpClient _client;
        private readonly ILogger<RedditService> _logger;

        public RedditService(HttpClient client, ILogger<RedditService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task GetPostsAsync(string keyword, string sortType, int? limit = null)
        {
            try
            {
                var response = await _client.GetAsync($"search.json?q={keyword}&sort={sortType}&type=posts");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get Reddit posts for {keyword} with status code {response.StatusCode}");
                    _logger.LogError(JsonConvert.SerializeObject(response, Formatting.Indented));
                    return;
                }

                _logger.LogInformation($"Successfully got Reddit posts for {keyword}");
                _logger.LogInformation(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get Reddit posts for {keyword}");
                _logger.LogError(ex.Message);
                throw;
            }
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
