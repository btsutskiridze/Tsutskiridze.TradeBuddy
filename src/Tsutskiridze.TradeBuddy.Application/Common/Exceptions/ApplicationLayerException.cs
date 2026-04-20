using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Application.Common.Exceptions;

public class ApplicationLayerException : BaseException
{
    public ApplicationLayerException(string message, int statusCode = 400, Exception? inner = null) : base(message, statusCode, inner)
    {
    }
}