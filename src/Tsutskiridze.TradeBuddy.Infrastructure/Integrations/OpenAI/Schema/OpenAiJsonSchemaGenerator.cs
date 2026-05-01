using System.Collections.Concurrent;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI.Schema
{
    public class OpenAiJsonSchemaGenerator : IOpenAiJsonSchemaGenerator
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
                _generator.GenerationProviders.Add(new OpenAiJsonSchemaGeneratorProvider());
                JSchema responseSchema = _generator.Generate(t);
                return responseSchema.ToString();
            });
        }
    }
}

