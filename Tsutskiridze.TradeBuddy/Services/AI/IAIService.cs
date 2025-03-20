namespace Tsutskiridze.TradeBuddy.Services.AI
{
    public interface IAIService
    {
        public Task<T> Ask<T>(string prompt);
    }
}
