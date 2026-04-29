namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;

public interface IDbExceptionClassifier
{
    public bool TryClassify(Exception exception, out PersistenceError error);
    bool IsUniqueConstraintViolation(Exception exception, out string constraintName);
    bool IsForeignKeyViolation(Exception exception);
    bool IsDeadlock(Exception exception);
}