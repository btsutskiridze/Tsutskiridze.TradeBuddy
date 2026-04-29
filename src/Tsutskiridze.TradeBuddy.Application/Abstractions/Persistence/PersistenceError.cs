namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;

public sealed record PersistenceError(PersistenceErrorCode Code)
{
    public static PersistenceError None => new PersistenceError(PersistenceErrorCode.None);
}