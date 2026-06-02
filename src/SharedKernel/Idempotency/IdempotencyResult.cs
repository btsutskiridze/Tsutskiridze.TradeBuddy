namespace SharedKernel.Idempotency;

public sealed record IdempotencyResult<TResult>(TResult Result, int StatusCode);