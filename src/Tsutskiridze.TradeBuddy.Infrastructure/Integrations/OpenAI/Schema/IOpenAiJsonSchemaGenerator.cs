namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI.Schema
{
    public interface IOpenAiJsonSchemaGenerator
    {
        string FromType(Type type);
    }
}
