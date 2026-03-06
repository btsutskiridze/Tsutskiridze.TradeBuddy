using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.Collections.Concurrent;
using Tsutskiridze.TradeBuddy.Application.Contracts.Services.AI.OpenAI;

namespace Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI.JsSchema
{
    public class OpenaiJsSchemaGenerator : IOpenaiJsSchemaGenerator
    {
        private readonly ConcurrentDictionary<Type, string> _cache = new();

        private static JSchemaGenerator _generator = new JSchemaGenerator
        {
            SchemaLocationHandling = SchemaLocationHandling.Inline,
            DefaultRequired = Newtonsoft.Json.Required.Always
        };

        public string FromType(Type type)
        {
            return _cache.GetOrAdd(type, t =>
            {
                _generator.GenerationProviders.Add(new OpenaiJsSchemaGenerationProvider());
                JSchema responseSchema = _generator.Generate(t);
                return responseSchema.ToString();
            });
        }
    }
}
