using Tsutskiridze.TradeBuddy.Application.DTOs.Persistence;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;

public interface IDbExceptionClassifier
{
    public bool TryClassify(Exception exception, out PersistenceErrorDto error);
    bool IsUniqueConstraintViolation(Exception exception, out string constraintName);
    bool IsForeignKeyViolation(Exception exception);
    bool IsDeadlock(Exception exception);
}