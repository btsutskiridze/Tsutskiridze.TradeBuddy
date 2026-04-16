namespace Tsutskiridze.TradeBuddy.Application.Exceptions;

public sealed class ConcurrencyConflictException : ApplicationLayerException
{
    public ConcurrencyConflictException(string message, Exception? inner = null)
        : base(message, 409, inner)
    {
    }
}