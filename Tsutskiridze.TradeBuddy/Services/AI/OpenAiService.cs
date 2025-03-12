using Microsoft.Extensions.Options;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenAI.Chat;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Tsutskiridze.TradeBuddy.Models;

namespace Tsutskiridze.TradeBuddy.Services.AI
{
    public class OpenAiService : IAIService
    {
        private static readonly string _apiKey = SecretsManager.GetSecret("AI:OpenAi:ApiKey");
        private static readonly string _modelID = SecretsManager.GetSecret("AI:OpenAi:ModelID");

        private readonly ChatClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OpenAiService(IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _client = new ChatClient(_modelID, _apiKey);
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }
        public class OpenaiJSSchemaGenerationProvider : JSchemaGenerationProvider
        {
            // This provider will handle any class type.
            public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
            {
                return IsClass(context.ObjectType);
            }

            private bool IsClass(Type type)
            {
                if (type == typeof(string) || type.IsPrimitive
                    || type.IsEnum || type.IsValueType || type.IsArray || type.IsInterface || type.IsAbstract || type.IsGenericType || type.IsGenericTypeDefinition)
                    return false;
                return type.IsClass;
            }

            public override JSchema GetSchema(JSchemaTypeGenerationContext context)
            {
                return GenerateSchema(context.ObjectType.GetProperties(), context);
            }

            private JSchema GenerateSchema(PropertyInfo[] properties, JSchemaTypeGenerationContext context)
            {
                // Create a new schema for object types and disable additional properties.
                var schema = new JSchema
                {
                    Type = JSchemaType.Object,
                    AllowAdditionalProperties = false
                };

                foreach (var property in properties)
                {
                    var propertySchema = IsClass(property.PropertyType)
                        ? GenerateSchema(property.PropertyType.GetProperties(), context)
                        : context.Generator.Generate(property.PropertyType);

                    schema.Properties.Add(property.Name, propertySchema);

                    if (property.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                    {
                        schema.Required.Add(property.Name);
                    }
                    //todo: description property
                }

                return schema;
            }
        }

        public async Task<T?> Ask<T>(string prompt)
        {

            JSchemaGenerator generator = new JSchemaGenerator
            {
                SchemaLocationHandling = SchemaLocationHandling.Inline,
                DefaultRequired = Newtonsoft.Json.Required.Always
            };

            // Add your custom provider so it is used for all object types, including nested ones
            generator.GenerationProviders.Add(new OpenaiJSSchemaGenerationProvider());

            JSchema responseSchema = generator.Generate(typeof(StockAnalysis));

            Console.WriteLine("========================================");
            Console.WriteLine(responseSchema.ToString());
            Console.WriteLine("========================================");

            List<ChatMessage> messages =
            [
                    new UserChatMessage(prompt),
            ];

            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 500,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "stock_analysis",
                    jsonSchema: BinaryData.FromString(responseSchema.ToString()),
                    //jsonSchema: BinaryData.FromBytes(
                    //    """
                    //        {
                    //            "type": "object",
                    //            "properties":{
                    //                "CurrentPrice":{
                    //                    "type":"string"
                    //                },
                    //                "NewsSentimentAnalysis":{
                    //                    "type":"object",
                    //                    "properties":{
                    //                        "Google":{
                    //                            "type":"string"
                    //                        },
                    //                        "Reddit":{
                    //                            "type":"string"
                    //                        },
                    //                        "Yahoo":{
                    //                            "type":"string"
                    //                        },
                    //                        "Finnhub":{
                    //                            "type":"string"
                    //                        }
                    //                    },
                    //                    "required":[
                    //                        "Google",
                    //                        "Reddit",
                    //                        "Yahoo",
                    //                        "Finnhub"
                    //                    ],
                    //                    "additionalProperties": false
                    //                },
                    //                "AiAnalysis":{
                    //                    "type":"string"
                    //                },
                    //                "Reasoning":{
                    //                    "type":"array",
                    //                    "items":{
                    //                        "type":"string"
                    //                    }
                    //                }
                    //            },
                    //            "required":[
                    //                "CurrentPrice",
                    //                "NewsSentimentAnalysis",
                    //                "AiAnalysis",
                    //                "Reasoning"
                    //            ],
                    //            "additionalProperties": false
                    //        }
                    //    """u8.ToArray()
                    //),
                    jsonSchemaIsStrict: true
                )
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

            T? result = JsonSerializer.Deserialize<T>(completion.Content[0].Text, _jsonSerializerOptions);


            Console.WriteLine("========================================");
            Console.WriteLine(JsonSerializer.Serialize(completion.Content, _jsonSerializerOptions));
            Console.WriteLine("========================================");


            Console.WriteLine("========================================");
            Console.WriteLine(JsonSerializer.Serialize(result, _jsonSerializerOptions));
            Console.WriteLine("========================================");

            return result;
        }
    }
}
