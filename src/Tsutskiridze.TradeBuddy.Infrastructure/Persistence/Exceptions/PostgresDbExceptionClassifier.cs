using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;
using Tsutskiridze.TradeBuddy.Application.Common.Enums;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;

public sealed class PostgresDbExceptionClassifier : IDbExceptionClassifier
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

    public bool TryClassify(Exception exception, out PersistenceError error)
    {
        error = PersistenceError.None;

        if (exception is not DbUpdateException dbUpdateException)
        {
            return false;
        }
        
        if (dbUpdateException.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        if (postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }

        error = postgresException.ConstraintName switch
        {
            PostgresConstraintNames.PriceAlertsUniqueBusinessKey
                => new PersistenceError(
                    PersistenceErrorCode.DuplicatePriceAlert
                ),

            PostgresConstraintNames.StocksSymbol
                => new PersistenceError(
                    PersistenceErrorCode.DuplicateStockSymbol
                ),
            _
                => PersistenceError.None
        };

        return error.Code != PersistenceErrorCode.None;
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