namespace Tsutskiridze.TradeBuddy.Application.Interfaces.AI
{
    public interface IAIService
    {
        public Task<T> Ask<T>(string prompt);
    }
}
