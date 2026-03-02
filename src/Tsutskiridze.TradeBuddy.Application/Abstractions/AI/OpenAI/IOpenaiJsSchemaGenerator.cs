namespace Tsutskiridze.TradeBuddy.Application.Abstractions.AI.OpenAI
{
    public interface IOpenaiJsSchemaGenerator
    {
        string FromType(Type type);
    }
}
