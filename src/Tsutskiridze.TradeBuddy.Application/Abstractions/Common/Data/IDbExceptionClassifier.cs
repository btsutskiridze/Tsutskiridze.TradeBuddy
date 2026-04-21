namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Common.Data;

public interface IDbExceptionClassifier
{
    bool IsUniqueConstraintViolation(Exception exception, out string constraintName);
    bool IsForeignKeyViolation(Exception exception);
    bool IsDeadlock(Exception exception);
}