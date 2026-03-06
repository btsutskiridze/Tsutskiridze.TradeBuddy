namespace Tsutskiridze.TradeBuddy.Application.Contracts.Services.AI.OpenAI
{
    public interface IOpenaiJsSchemaGenerator
    {
        string FromType(Type type);
    }
}
