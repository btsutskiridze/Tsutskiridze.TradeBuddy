using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Utilities;


public sealed class EfCoreDbExceptionClassifier : IDbExceptionClassifier
{
    public bool IsUniqueConstraintViolation(DbUpdateException ex, out string constraintName)
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

    public bool IsForeignKeyViolation(DbUpdateException exception)
    {
        throw new NotImplementedException();
    }

    public bool IsDeadlock(Exception exception)
    {
        throw new NotImplementedException();
    }
}
