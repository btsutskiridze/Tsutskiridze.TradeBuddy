namespace SharedKernel.Idempotency;

public interface IIdempotency
{
    Task<TResult> Execute<TRequest, TResult>(
        string key,
        string? scope,
        TRequest request,
        Func<CancellationToken, Task<IdempotencyResult<TResult>>> idempotentAction,
        CancellationToken ct);
    
    Task<int> Execute<TRequest>(
        string key,
        string? scope,
        TRequest request,
        Func<CancellationToken, Task<int>> idempotentAction,
        CancellationToken ct);
}