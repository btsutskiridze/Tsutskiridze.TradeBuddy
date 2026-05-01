using System.Data;

namespace SharedKernel.Data;

public interface IUnitOfWork
{
    Task<IAppDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}