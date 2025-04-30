using Newtonsoft.Json;
using OpenAI.Chat;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI.OpenAI;

namespace Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI
{
    public class OpenAIService : IAIService
    {
        private readonly ChatClient _client;

        private readonly IOpenaiJsSchemaGenerator _openaiJsSchemaGenerator;

        public OpenAIService(ChatClient client, IOpenaiJsSchemaGenerator openaiJsSchemaGenerator)
        {
            _client = client;
            _openaiJsSchemaGenerator = openaiJsSchemaGenerator;
        }

        public async Task<T> Ask<T>(string prompt)
        {
            List<ChatMessage> messages =
            [
                    new UserChatMessage(prompt),
            ];

            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 500,
                Temperature = (float?)0.2,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "stock_analysis",
                    jsonSchema: BinaryData.FromString(_openaiJsSchemaGenerator.FromType(typeof(T))),
                    jsonSchemaIsStrict: true
                )
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

            var result = JsonConvert.DeserializeObject<T>(completion.Content[0].Text);

            if (result == null)
            {
                throw new Exception("AI returned null");
            }

            return result;
        }
    }
}
