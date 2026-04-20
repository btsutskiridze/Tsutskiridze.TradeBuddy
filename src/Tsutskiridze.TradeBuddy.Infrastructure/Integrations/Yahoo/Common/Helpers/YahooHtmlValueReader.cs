using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Helpers;

public static class YahooHtmlValueReader
{
    public static string Clean(string? value) =>
        HtmlEntity.DeEntitize(value ?? string.Empty).Trim();

    public static bool IsQuoteTitleMatch(HtmlDocument document, string symbol, out string titleText)
    {
        var titleNode = document.DocumentNode.SelectSingleNode("//section[@data-testid='quote-title']/h1");
        titleText = Clean(titleNode?.InnerText);

        return !string.IsNullOrWhiteSpace(titleText) &&
               Regex.IsMatch(
                   titleText,
                   $@"\(\s*{Regex.Escape(symbol)}\s*\)\s*$",
                   RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    public static string GetNodeText(HtmlDocument document, string xpath)
    {
        var node = document.DocumentNode.SelectSingleNode(xpath);
        return Clean(node?.InnerText);
    }

    public static string GetNodeValueOrText(HtmlDocument document, string xpath)
    {
        var node = document.DocumentNode.SelectSingleNode(xpath);
        return Clean(node?.GetAttributeValue("data-value", null) ?? node?.InnerText);
    }

    public static string GetQuoteSummaryValue(HtmlDocument document, string label)
    {
        var node = document.DocumentNode.SelectSingleNode($"//li[.//span[@title='{label}']]//fin-streamer");
        return Clean(node?.GetAttributeValue("data-value", null) ?? node?.InnerText);
    }

    public static string GetTableRowLastCellValue(HtmlDocument document, string rowLabel)
    {
        var node = document.DocumentNode.SelectSingleNode($"//tr[.//td[contains(normalize-space(.), '{rowLabel}')]]/td[last()]");
        return Clean(node?.InnerText);
    }

    public static string GetFinancialStatementRowValue(HtmlDocument document, params string[] labels)
    {
        var rowNodes = document.DocumentNode.SelectNodes("//div[contains(@class,'row')][.//div[contains(@class,'rowTitle')]]");
        if (rowNodes is null)
            return string.Empty;

        foreach (var row in rowNodes)
        {
            var rowTitleNode = row.SelectSingleNode(".//div[contains(@class,'rowTitle')]");
            if (rowTitleNode is null)
                continue;

            var label = rowTitleNode.GetAttributeValue("title", null);
            if (string.IsNullOrWhiteSpace(label))
                label = Clean(rowTitleNode.InnerText);

            if (!labels.Any(candidate => string.Equals(candidate, label, StringComparison.OrdinalIgnoreCase)))
                continue;

            var valueNodes = row.SelectNodes("./div[contains(@class,'column') and not(contains(@class,'sticky'))]");
            if (valueNodes is null)
                continue;

            foreach (var valueNode in valueNodes)
            {
                var value = Clean(valueNode.InnerText);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return string.Empty;
    }

    public static string GetEarningsAnnouncementFromHtml(HtmlDocument document)
    {
        var earningsNode = document.DocumentNode.SelectSingleNode("//li[.//span[contains(@title, 'Earnings Date')]]");
        if (earningsNode is null)
            return string.Empty;

        var finStreamer = earningsNode.SelectSingleNode(".//fin-streamer");
        if (finStreamer is not null)
            return Clean(finStreamer.InnerText);

        var spans = earningsNode.SelectNodes(".//span");
        if (spans is null)
            return string.Empty;

        var values = spans
            .Select(node => Clean(node.InnerText))
            .Where(value => !string.IsNullOrWhiteSpace(value) && !value.StartsWith("Earnings Date", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return values.Count > 0 ? values[^1] : string.Empty;
    }
}

