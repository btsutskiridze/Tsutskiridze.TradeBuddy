using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;

namespace Tsutskiridze.TradeBuddy.Infrastructure.AI.Gemini
{
    public class GeminiService : IAiService
    {
        private readonly GeminiOptions _options;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public GeminiService(HttpClient client, IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IOptions<GeminiOptions> options)
        {
            _client = client;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
            _options = options.Value;
        }

        public async Task<T?> Ask<T>(string prompt)
        {
            var request = new
            {
                contents = new
                {
                    parts = new
                    {
                        text = prompt
                    }
                }
            };
            string json = JsonSerializer.Serialize(request, _jsonSerializerOptions);


            var response = await _client.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_options.ModelID}:generateContent?key={_options.ApiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseJson, _jsonSerializerOptions);
        }

        public async Task<string> CountTokens(string prompt)
        {
            var request = new
            {
                contents = new
                {
                    parts = new
                    {
                        text = prompt
                    }
                }
            };

            string json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var response = await _client.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_options.ModelID}:countTokens?key={_options.ApiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }

    }
}
