using Microsoft.EntityFrameworkCore;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;

public interface IDbExceptionClassifier
{
    bool IsUniqueConstraintViolation(DbUpdateException exception, out string constraintName);
    bool IsForeignKeyViolation(DbUpdateException exception);
    bool IsDeadlock(Exception exception);
}