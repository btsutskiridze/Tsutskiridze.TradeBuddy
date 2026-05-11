namespace Tsutskiridze.TradeBuddy.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

public static class UriExtensions
{
    public static string AsQueryString(this IDictionary<string, string> queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
        {
            return string.Empty;
        }

        // Safely URL-encode both keys and values, then join them with '&'
        var encodedParams = queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");

        var queryString = string.Join("&", encodedParams);
        
        return queryString;
    }
}