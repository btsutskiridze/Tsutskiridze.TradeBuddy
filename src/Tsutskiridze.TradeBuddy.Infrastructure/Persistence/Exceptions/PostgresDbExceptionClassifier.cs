using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;

internal sealed class PostgresDbExceptionClassifier : IDbExceptionClassifier
{
    public Exception? Translate(DbUpdateException exception)
    {
        if (exception is DbUpdateConcurrencyException)
        {
            return new ConcurrencyConflictException(
                "The data was changed by another process.",
                exception);
        }

        if (exception.InnerException is not PostgresException pg)
        {
            return null;
        }

        if (pg.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return null;
        }

        return pg.ConstraintName switch
        {
            PostgresConstraintNames.PriceAlertsUniqueBusinessKey
                => new DuplicatePriceAlertException(exception),

            PostgresConstraintNames.StocksSymbol
                => new DuplicateStockSymbolException(exception),

            _ => null
        };
    }
}