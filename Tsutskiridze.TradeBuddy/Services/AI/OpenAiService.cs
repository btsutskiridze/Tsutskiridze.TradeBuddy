using Newtonsoft.Json;
using OpenAI.Chat;
using Tsutskiridze.TradeBuddy.Services.JsSchema;

namespace Tsutskiridze.TradeBuddy.Services.AI
{
    public class OpenAiService : IAIService
    {
        private static readonly string _apiKey = SecretsManager.GetSecret("AI:OpenAi:ApiKey");
        private static readonly string _modelID = SecretsManager.GetSecret("AI:OpenAi:ModelID");

        private readonly ChatClient _client;
        public OpenAiService()
        {
            _client = new ChatClient(_modelID, _apiKey);
        }

        public async Task<T?> Ask<T>(string prompt)
        {
            List<ChatMessage> messages =
            [
                    new UserChatMessage(prompt),
            ];

            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 500,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "stock_analysis",
                    jsonSchema: BinaryData.FromString(OpenaiJSSchemaGenerator.FromType(typeof(T))),
                    jsonSchemaIsStrict: true
                )
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

            T? result = JsonConvert.DeserializeObject<T>(completion.Content[0].Text);

            return result;
        }
    }
}
