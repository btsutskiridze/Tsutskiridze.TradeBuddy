namespace Tsutskiridze.TradeBuddy.Application.Common.Exceptions;

public class ResourceNotFoundException : ApplicationLayerException
{
    public ResourceNotFoundException(string message) : base(message, 404)
    {
    }
}