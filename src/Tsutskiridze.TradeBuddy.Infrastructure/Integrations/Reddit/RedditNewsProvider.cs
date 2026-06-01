using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Extensions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit
{
    public class RedditNewsProvider : IRedditNewsProvider
    {
        private readonly RedditOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RedditNewsProvider> _logger;

        private record AccessToken(string Token, DateTimeOffset ExpiresIn);

        private static AccessToken? _accessToken;

        public RedditNewsProvider(HttpClient client, ILogger<RedditNewsProvider> logger,
            IOptions<RedditOptions> options)
        {
            _options = options.Value;
            _httpClient = client;
            _logger = logger;
        }

        public async Task<List<RedditPost>?> GetRedditPosts(string keyword, SortType sortType, int? limit = null)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "q", keyword },
                { "sort", sortType.ToString().ToLower() },
                { "type", "posts" }
            };
            if (limit.HasValue)
            {
                queryParams.Add("limit", limit.Value.ToString());
            }

            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_options.SearchEndpoint}?{queryParams.AsQueryString()}"
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

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
            if (_accessToken != null && _accessToken.ExpiresIn > DateTimeOffset.Now.AddMinutes(5))
            {
                return _accessToken.Token;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.AuthEndpoint);
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", _options.Username },
                { "password", _options.Password }
            };
            request.Content = new FormUrlEncodedContent(formData);

            var authHeader =
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            var accessToken = json["access_token"]?.Value<string>();
            var expiresIn = json["expires_in"]?.Value<int>() ?? 0;

            if (string.IsNullOrEmpty(accessToken) || expiresIn == 0)
            {
                throw new InfrastructureException("Error getting access token");
            }

            _accessToken = new AccessToken(
                accessToken,
                DateTimeOffset.Now.AddSeconds(expiresIn)
            );

            return accessToken;
        }
    }
}