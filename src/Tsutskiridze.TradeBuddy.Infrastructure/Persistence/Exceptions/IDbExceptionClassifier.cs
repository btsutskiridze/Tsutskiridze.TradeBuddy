using Microsoft.EntityFrameworkCore;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;

internal interface IDbExceptionClassifier
{
    Exception? Translate(DbUpdateException exception);
}

