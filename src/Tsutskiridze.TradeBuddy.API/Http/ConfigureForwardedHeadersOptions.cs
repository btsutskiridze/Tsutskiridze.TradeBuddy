using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using IPNetwork = System.Net.IPNetwork;

namespace Tsutskiridze.TradeBuddy.API.Http;

internal sealed class ConfigureForwardedHeadersOptions : IConfigureOptions<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>
{
    private readonly ForwardedHeadersOptions _options;

    public ConfigureForwardedHeadersOptions(IOptions<ForwardedHeadersOptions> settings)
    {
        _options = settings.Value;
    }

    public void Configure(Microsoft.AspNetCore.Builder.ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        foreach (var proxy in _options.KnownProxies)
        {
            if (IPAddress.TryParse(proxy, out var ipAddress))
            {
                options.KnownProxies.Add(ipAddress);
            }
        }

        foreach (var network in _options.KnownNetworks)
        {
            if (IPNetwork.TryParse(network, out var ipNetwork))
            {
                options.KnownIPNetworks.Add(ipNetwork);
            }
        }
    }
}
