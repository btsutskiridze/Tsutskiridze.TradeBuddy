using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Idempotency;

public static class RequestHashing
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static byte[] ComputeRequestHash<TRequest>(TRequest request)
    {
        if (request is null)
            throw new InvalidOperationException("Request cannot be null");

        var jsonNode = JsonSerializer.SerializeToNode(
            request,
            JsonOptions);

        if (jsonNode is null)
            throw new InvalidOperationException("Request cannot be serialized for hashing.");

        var canonicalJson = Canonicalize(jsonNode);

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(
            canonicalJson,
            JsonOptions);

        return SHA256.HashData(jsonBytes);
    }

    private static JsonNode? Canonicalize(JsonNode? node)
    {
        return node switch
        {
            JsonObject obj => CanonicalizeObject(obj),
            JsonArray arr => CanonicalizeArray(arr),
            JsonValue value => value.DeepClone(),
            null => null,
            _ => throw new NotSupportedException($"Unsupported JSON node type: {node.GetType().Name}")
        };
    }

    private static JsonNode? CanonicalizeNode(JsonNode? node)
    {
        return node switch
        {
            null => null,
            JsonObject jsonObject => CanonicalizeObject(jsonObject),
            JsonArray jsonArray => CanonicalizeArray(jsonArray),
            JsonValue jsonValue => jsonValue.DeepClone(),
            _ => throw new NotSupportedException($"Unsupported JSON node type: {node.GetType().Name}")
        };
    }

    private static JsonObject CanonicalizeObject(JsonObject jsonObject)
    {
        var result = new JsonObject();

        foreach (var property in jsonObject.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            result[property.Key] = CanonicalizeNode(property.Value);
        }

        return result;
    }

    private static JsonArray CanonicalizeArray(JsonArray jsonArray)
    {
        var result = new JsonArray();

        foreach (var item in jsonArray)
        {
            result.Add(CanonicalizeNode(item));
        }

        return result;
    }
}