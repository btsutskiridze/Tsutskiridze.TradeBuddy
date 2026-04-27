using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel.Data;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public sealed class EfAppDbTransaction : IAppDbTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _completed;

    public EfAppDbTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _transaction.CommitAsync(ct);
        _completed = true;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        await _transaction.RollbackAsync(ct);
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
            await _transaction.RollbackAsync();

        await _transaction.DisposeAsync();
    }
}