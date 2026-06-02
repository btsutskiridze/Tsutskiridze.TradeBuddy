using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Idempotency;
using Tsutskiridze.TradeBuddy.Infrastructure.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Idempotency;

internal class IdempotencyService : IIdempotency
{
    private const short LockSeconds = 120;
    private readonly JsonSerializerOptions _serializerOptions = InfraJsonSerializerOptions.CreateOptions();
    private const string DefaultScope = "global";

    private readonly AppDbContext _db;
    private readonly IDbExceptionClassifier _exClassifier;

    public IdempotencyService(AppDbContext db, IDbExceptionClassifier exClassifier)
    {
        _db = db;
        _exClassifier = exClassifier;
    }

    public async Task<TResult> Execute<TRequest, TResult>(
        string key,
        string? scope,
        TRequest request,
        Func<CancellationToken, Task<IdempotencyResult<TResult>>> idempotentAction,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        key = key.Trim();
        scope = string.IsNullOrWhiteSpace(scope) ? DefaultScope : scope;

        var now = DateTime.UtcNow;
        var lockedUntil = now.AddSeconds(LockSeconds);

        var keyHash = ComputeHash(key.Trim());
        var scopeHash = ComputeHash(scope);
        var requestHash = RequestHashing.ComputeRequestHash(request);
        var record = await GetExistingRecord(keyHash, scopeHash, ct);

        if (record is not null)
        {
            if (!record.RequestHash.SequenceEqual(requestHash))
            {
                throw new InfrastructureException(
                    "Idempotency key was already used with a different request.",
                    (int)HttpStatusCode.Conflict
                );
            }

            if (TryGetIdempotencyResult<TResult>(record, out var cachedResult))
                return cachedResult;

            record.ReProcess(ComputeLockedUntil());
        }
        else
        {
            record = IdempotencyRecord.Started(
                keyHash,
                scopeHash,
                key,
                scope,
                requestHash,
                lockedUntil);

            await _db.IdempotencyRecords.AddAsync(record, ct);
        }

        await SaveChangesAsync(ct);

        try
        {
            var (result, statusCode) = await idempotentAction(ct);
            var resultJson = JsonSerializer.Serialize(result, _serializerOptions);

            record.Completed(resultJson, statusCode);
            await SaveChangesAsync(CancellationToken.None);
            return result;
        }
        catch (BaseException ex) when (ex.StatusCode is >= 400 and < 500)
        {
            record.Failed(ex.Message, ex.StatusCode);
            await SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_exClassifier.Translate(ex) is { } translated)
        {
            throw translated;
        }
    }
    
    private bool TryGetIdempotencyResult<TResult>(
        IdempotencyRecord record,
        out TResult result)
    {
        switch (record.Status)
        {
            case IdempotencyStatus.Processing when !IsLockedUntilExpired(record.LockedUntil!.Value):
                throw new InfrastructureException(
                    "Request is being processed",
                    (int)HttpStatusCode.Conflict);

            case IdempotencyStatus.Failed:
                throw new InfrastructureException(
                    record.Error!,
                    record.StatusCode!.Value);

            case IdempotencyStatus.Completed:
                result = JsonSerializer.Deserialize<TResult>(
                    record.ResponseJson!,
                    _serializerOptions)!;
                return true;

            default:
                result = default!;
                return false;
        }
    }

    private async Task<IdempotencyRecord?> GetExistingRecord(byte[] keyHash, byte[] scopeHash, CancellationToken ct)
    {
        var record = await _db.IdempotencyRecords
            .Where(x =>
                x.KeyHash == keyHash && x.ScopeHash == scopeHash
            ).FirstOrDefaultAsync(ct);
        return record;
    }

    private DateTime ComputeLockedUntil()
    {
        var addon = TimeSpan.FromSeconds(LockSeconds);

        return DateTime.UtcNow + addon;
    }

    private static byte[] ComputeHash(string value)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }

    private static bool IsLockedUntilExpired(DateTime lockedUntil)
    {
        return lockedUntil < DateTime.UtcNow;
    }
}