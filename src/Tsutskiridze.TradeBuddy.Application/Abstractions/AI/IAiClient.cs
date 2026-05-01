namespace Tsutskiridze.TradeBuddy.Application.Abstractions.AI
{
    public interface IAiClient
    {
        public Task<T> Ask<T>(string prompt);
    }
}
