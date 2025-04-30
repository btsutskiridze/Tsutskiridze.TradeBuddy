using Newtonsoft.Json;
using OpenAI.Chat;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI.JsSchema;

namespace Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI
{
    public class OpenAIService : IAIService
    {
        private readonly ChatClient _client;

        public OpenAIService(ChatClient client)
        {
            _client = client;
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
                Temperature = 1,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "stock_analysis",
                    jsonSchema: BinaryData.FromString(OpenaiJsSchemaGenerator.FromType(typeof(T))),
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
