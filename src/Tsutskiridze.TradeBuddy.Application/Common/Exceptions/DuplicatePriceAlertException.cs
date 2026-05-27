namespace Tsutskiridze.TradeBuddy.Application.Common.Exceptions;

public sealed class DuplicatePriceAlertException : ApplicationLayerException
{
    public DuplicatePriceAlertException(Exception? inner = null)
        : base("Alert already exists.", 409, inner)
    {
    }
}

