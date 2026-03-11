using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Application.Exceptions;

public class ApplicationLayerException : BaseException
{
    public ApplicationLayerException(string message, int statusCode = 400) : base(message, statusCode)
    {
    }
}