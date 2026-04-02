using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Infrastructure.News.Reddit
{
    public class RedditClient : IRedditNewsProvider
    {
        private readonly RedditOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RedditClient> _logger;

        private record AccessToken(string Token, DateTimeOffset ExpiresIn);

        private static AccessToken? _accessToken;

        public RedditClient(HttpClient client, ILogger<RedditClient> logger, IOptions<RedditOptions> options)
        {
            _options = options.Value;
            _httpClient = client;
            _logger = logger;
        }

        public async Task<List<RedditPostDto>?> GetRedditPosts(string keyword, RedditSortType sortType, int? limit = null)
        {
            string limitQuery = limit.HasValue ? $"&limit={limit}" : string.Empty;

            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_options.SearchEndpoint}?q={keyword}&sort={sortType.ToString().ToLower()}&type=posts{limitQuery}"
            );
            requestMessage.Headers.Add("User-Agent", _options.UserAgent);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonString);

            if (json["data"]?["children"] == null)
            {
                return null;
            }

            var posts = new List<RedditPostDto>();

            foreach (var item in json["data"]!["children"]!)
            {
                try
                {
                    var score = item["data"]?["score"]?.Value<int>() ?? 0;
                    var comments = item["data"]?["num_comments"]?.Value<int>() ?? 0;

                    if (score >= 5 && comments >= 3)
                    {
                        var body = item["data"]?["selftext"]?.Value<string>() ?? string.Empty;
                        posts.Add(new RedditPostDto
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
            if (_accessToken != null && _accessToken.ExpiresIn > DateTimeOffset.Now.AddMinutes(5))
            {
                return _accessToken.Token;
            }

            var requestBody = new StringContent(
                $"grant_type=password&username={Uri.EscapeDataString(_options.Username)}&password={Uri.EscapeDataString(_options.Password)}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);

            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);

            try
            {
                var response = await _httpClient.PostAsync(_options.AuthEndpoint, requestBody);
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
                    DateTimeOffset.Now.AddSeconds(expiresIn)
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

