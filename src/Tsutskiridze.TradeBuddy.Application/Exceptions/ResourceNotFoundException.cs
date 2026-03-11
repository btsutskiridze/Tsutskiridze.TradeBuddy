namespace Tsutskiridze.TradeBuddy.Application.Exceptions;

public class ResourceNotFoundException : ApplicationLayerException
{
    public ResourceNotFoundException(string message) : base(message, 404)
    {
    }
}