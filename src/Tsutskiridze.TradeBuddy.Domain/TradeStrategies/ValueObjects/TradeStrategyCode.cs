using System.Globalization;
using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

public sealed record TradeStrategyCode : ValueObject
{
    private const string Prefix = "ts_";

    public int Id { get; }

    private TradeStrategyCode()
    {
    }

    public TradeStrategyCode(int id)
    {
        if (id <= 0)
        {
            throw new DomainException("Trade strategy id must be positive.");
        }

        Id = id;
    }

    public static TradeStrategyCode FromId(int id)
    {
        return new TradeStrategyCode(id);
    }

    public static TradeStrategyCode Parse(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Invalid trade strategy code.");
        }

        var span = code.AsSpan().Trim();
        if (!span.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Invalid trade strategy code.");
        }

        var idText = span[Prefix.Length..];
        if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            throw new DomainException("Invalid trade strategy code.");
        }

        return new TradeStrategyCode(id);
    }

    public override string ToString()
    {
        return $"{Prefix}{Id}";
    }
}
