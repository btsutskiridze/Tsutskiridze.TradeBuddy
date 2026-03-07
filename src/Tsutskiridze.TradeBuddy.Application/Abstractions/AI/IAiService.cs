namespace Tsutskiridze.TradeBuddy.Application.Abstractions.AI
{
    public interface IAiService
    {
        public Task<T> Ask<T>(string prompt);
    }
}
