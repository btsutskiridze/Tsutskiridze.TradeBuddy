using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.Persistence;

public sealed record PersistenceErrorDto(PersistenceErrorCode Code)
{
    public static PersistenceErrorDto None => new PersistenceErrorDto(PersistenceErrorCode.None);
}