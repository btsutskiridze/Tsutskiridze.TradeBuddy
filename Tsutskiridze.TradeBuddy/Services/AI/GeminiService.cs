using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Tsutskiridze.TradeBuddy.Services.AI
{
    public class GeminiService : IAIService
    {
        private static readonly string _apiKey = SecretsManager.GetSecret("AI:Gemini:ApiKey");
        private static readonly string _modelID = SecretsManager.GetSecret("AI:Gemini:ModelID");

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public GeminiService(HttpClient client, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _client = client;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
                $"https://generativelanguage.googleapis.com/v1beta/models/{_modelID}:generateContent?key={_apiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            Console.WriteLine("===============================================");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            Console.WriteLine("===============================================");

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
                $"https://generativelanguage.googleapis.com/v1beta/models/{_modelID}:countTokens?key={_apiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            Console.WriteLine("===============================================");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            Console.WriteLine("===============================================");
            return await response.Content.ReadAsStringAsync();
        }

    }
}
