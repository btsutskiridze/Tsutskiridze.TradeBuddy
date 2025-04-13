using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI.JsSchema
{
    public static class OpenaiJsSchemaGenerator
    {
        private static JSchemaGenerator _generator = new JSchemaGenerator
        {
            SchemaLocationHandling = SchemaLocationHandling.Inline,
            DefaultRequired = Newtonsoft.Json.Required.Always
        };

        public static string FromType(Type type)
        {
            _generator.GenerationProviders.Add(new OpenaiJsSchemaGenerationProvider());

            JSchema responseSchema = _generator.Generate(type);

            return responseSchema.ToString();
        }
    }
}
