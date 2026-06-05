namespace Tsutskiridze.TradeBuddy.API.Http;

public sealed class ForwardedHeadersOptions
{
    public const string SectionName = "ForwardedHeaders";

    public List<string> KnownProxies { get; init; } = [];

    public List<string> KnownNetworks { get; init; } = [];
}
