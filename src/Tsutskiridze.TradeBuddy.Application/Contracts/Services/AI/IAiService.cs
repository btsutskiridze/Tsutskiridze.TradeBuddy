namespace Tsutskiridze.TradeBuddy.Application.Contracts.Services.AI
{
    public interface IAiService
    {
        public Task<T> Ask<T>(string prompt);
    }
}
