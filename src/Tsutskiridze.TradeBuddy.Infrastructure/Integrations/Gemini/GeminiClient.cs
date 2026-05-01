using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Infrastructure.Exceptions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Gemini
{
    public class GeminiClient : IAiClient
    {
        private readonly GeminiOptions _options;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public GeminiClient(HttpClient client, IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IOptions<GeminiOptions> options)
        {
            _client = client;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
            _options = options.Value;
        }

        public async Task<T> Ask<T>(string prompt)
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
            var result = JsonSerializer.Deserialize<T>(responseJson, _jsonSerializerOptions)
                         ?? throw new InfrastructureException("Failed to parse Gemini AI response");

            return result;
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