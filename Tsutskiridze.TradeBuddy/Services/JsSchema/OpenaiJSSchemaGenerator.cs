using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Tsutskiridze.TradeBuddy.Services.JsSchema
{
    public static class OpenaiJSSchemaGenerator
    {
        private static JSchemaGenerator _generator = new JSchemaGenerator
        {
            SchemaLocationHandling = SchemaLocationHandling.Inline,
            DefaultRequired = Newtonsoft.Json.Required.Always
        };

        public static string FromType(Type type)
        {
            _generator.GenerationProviders.Add(new OpenaiJSSchemaGenerationProvider());

            JSchema responseSchema = _generator.Generate(type);

            return responseSchema.ToString();
        }
    }
}
