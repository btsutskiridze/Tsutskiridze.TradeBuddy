namespace SharedKernel.Data;

public interface IAppDbTransaction:IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}