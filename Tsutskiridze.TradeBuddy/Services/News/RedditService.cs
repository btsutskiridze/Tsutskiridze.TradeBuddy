using Azure.Core;
using Newtonsoft.Json.Linq;
using System.Text;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;
using Tsutskiridze.TradeBuddy.Helpers;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class RedditService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RedditService> _logger;

        private static AccessToken? _accessToken;
        private static class RedditParams
        {
            public static readonly string AuthEndpoint = SecretsManager.GetSecret("Reddit:AuthEndpoint");
            public static readonly string SearchEndpoint = SecretsManager.GetSecret("Reddit:SearchEndpoint");
            public static readonly string ClientId = SecretsManager.GetSecret("Reddit:ClientId");
            public static readonly string ClientSecret = SecretsManager.GetSecret("Reddit:ClientSecret");
            public static readonly string Username = SecretsManager.GetSecret("Reddit:Username");
            public static readonly string Password = SecretsManager.GetSecret("Reddit:Password");
            public static readonly string UserAgent = SecretsManager.GetSecret("Reddit:UserAgent");
        }

        public RedditService(HttpClient client, ILogger<RedditService> logger)
        {
            _httpClient = client;
            _logger = logger;
        }

        public async Task<List<RedditPost>?> GetRedditPosts(string keyword, RedditSortType sortType, int? limit = null)
        {
            string limitQuery = limit.HasValue ? $"&limit={limit}" : string.Empty;

            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"{RedditParams.SearchEndpoint}?q={keyword}&sort={sortType.ToString().ToLower()}&type=posts{limitQuery}"
            );
            requestMessage.Headers.Add("User-Agent", RedditParams.UserAgent);
            requestMessage.Headers.Add("Authorization", $"Bearer {await GetAccessToken()}");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonString);

            if (json["data"]?["children"] == null)
            {
                return null;
            }

            var posts = new List<RedditPost>();

            foreach (var item in json["data"]!["children"]!)
            {
                try
                {
                    var score = item["data"]?["score"]?.Value<int>() ?? 0;
                    var comments = item["data"]?["num_comments"]?.Value<int>() ?? 0;

                    // Only include posts that meet your score/comments thresholds
                    if (score >= 5 && comments >= 3)
                    {
                        var body = item["data"]?["selftext"]?.Value<string>() ?? string.Empty;
                        posts.Add(new RedditPost
                        {
                            Title = item["data"]?["title"]?.Value<string>() ?? string.Empty,
                            Url = item["data"]?["url"]?.Value<string>() ?? string.Empty,
                            Body = body.Length > 350 ? body[..350] + "..." : body,
                            Score = score,
                            CommentsCount = comments,
                            CreateTime = DateTimeOffset
                                .FromUnixTimeSeconds(item["data"]?["created_utc"]?.Value<long>() ?? 0)
                                .ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
                catch
                {
                    _logger.LogWarning("Error parsing Reddit post");
                }

                if (limit.HasValue && posts.Count >= limit.Value)
                {
                    break;
                }
            }

            return posts.Count > 0 ? posts : null;
        }

        private async Task<string> GetAccessToken()
        {
            if (_accessToken != null && _accessToken.Value.ExpiresOn > DateTimeOffset.Now.AddMinutes(5))
            {
                return _accessToken.Value.Token;
            }

            var requestBody = new StringContent(
                $"grant_type=password&username={Uri.EscapeDataString(RedditParams.Username)}&password={Uri.EscapeDataString(RedditParams.Password)}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            _httpClient.DefaultRequestHeaders.Add("User-Agent", RedditParams.UserAgent);

            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{RedditParams.ClientId}:{RedditParams.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeaderValue}");

            try
            {
                var response = await _httpClient.PostAsync(RedditParams.AuthEndpoint, requestBody);
                response.EnsureSuccessStatusCode();

                var json = JObject.Parse(await response.Content.ReadAsStringAsync());

                var accessToken = json["access_token"]?.Value<string>();
                var expiresIn = json["expires_in"]?.Value<int>() ?? 0;

                if (string.IsNullOrEmpty(accessToken) || expiresIn == 0)
                {
                    throw new Exception("Error getting access token");
                }

                _accessToken = new AccessToken(
                    accessToken,
                    DateTimeOffset.Now.AddSeconds(expiresIn),
                    DateTimeOffset.Now.AddSeconds(expiresIn / 2)
                );

                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting access token: {message}", ex.Message);
                throw new Exception("Error getting access token");
            }
        }
    }
}
