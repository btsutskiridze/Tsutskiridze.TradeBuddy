using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

public sealed class InfraJsonSerializerOptions
{
    public JsonSerializerOptions Options { get; }

    public InfraJsonSerializerOptions()
    {
        Options = CreateOptions();
    }

    public static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Configure(options);
        return options;
    }

    public static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.WriteIndented = false;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        if (options.Converters.All(c => c is not JsonStringEnumConverter))
        {
            options.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        }
    }
}
