namespace Tsutskiridze.TradeBuddy.Application.Abstractions.AI
{
    public interface IAIService
    {
        public Task<T> Ask<T>(string prompt);
    }
}
