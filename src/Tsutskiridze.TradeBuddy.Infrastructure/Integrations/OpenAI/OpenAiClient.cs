using System.Text.Json;
using OpenAI.Chat;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Infrastructure.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI.Schema;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI
{
    public class OpenAiClient : IAiClient
    {
        private readonly ChatClient _client;

        private readonly IOpenAiJsonSchemaGenerator _openAiJsonSchemaGenerator;

        public OpenAiClient(ChatClient client, IOpenAiJsonSchemaGenerator openAiJsonSchemaGenerator)
        {
            _client = client;
            _openAiJsonSchemaGenerator = openAiJsonSchemaGenerator;
        }

        //todo: refactor this class as the ask isn't generic and is specific for stock analysis
        public async Task<T> Ask<T>(string prompt)
        {
            List<ChatMessage> messages =
            [
                new SystemChatMessage(
                    "You are an expert stock‐market analyst with 20 years of experience. " +
                    "Provide concise, data‐driven BUY, SELL or HOLD recommendations, " +
                    "backed by price trends, volume analysis, earnings, and news sentiment."
                ),
                new UserChatMessage(prompt),
            ];

            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 500,
                Temperature = (float?)0.2,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "stock_analysis",
                    jsonSchema: BinaryData.FromString(_openAiJsonSchemaGenerator.FromType(typeof(T))),
                    jsonSchemaIsStrict: true
                )
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

            var result = JsonSerializer.Deserialize<T>(completion.Content[0].Text)
                         ?? throw new InfrastructureException("Failed to parse OpenAI response.");

            return result;
        }
    }
}