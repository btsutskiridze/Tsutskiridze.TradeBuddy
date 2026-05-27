namespace Tsutskiridze.TradeBuddy.Application.Common.Exceptions;

public sealed class DuplicateStockSymbolException : ApplicationLayerException
{
    public DuplicateStockSymbolException(Exception? inner = null)
        : base("Stock symbol already exists.", 409, inner)
    {
    }
}

