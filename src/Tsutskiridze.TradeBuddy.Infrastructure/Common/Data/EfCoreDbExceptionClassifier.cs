using Npgsql;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Common.Data;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Common.Data;


public sealed class EfCoreDbExceptionClassifier : IDbExceptionClassifier
{
    public bool IsUniqueConstraintViolation(Exception ex, out string constraintName)
    {
        constraintName = string.Empty;

        if (ex.InnerException is PostgresException pg && 
            pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            constraintName = pg.ConstraintName ?? string.Empty;
            return true;
        }

        return false;
    }

    public bool IsForeignKeyViolation(Exception exception)
    {
        throw new NotImplementedException();
    }

    public bool IsDeadlock(Exception exception)
    {
        throw new NotImplementedException();
    }
}
