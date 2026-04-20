using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Common.Exceptions;

public class InfrastructureException : BaseException
{
    public InfrastructureException(string message, int statusCode = 500, Exception? inner = null) : base(message, statusCode, inner)
    {
    }
}
