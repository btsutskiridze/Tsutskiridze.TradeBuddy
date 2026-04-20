using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Helpers;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing;

public class YahooFinancialsPageParser : IYahooFinancialsPageParser
{
    public YahooFinancialsPageParser(
        IYahooJsonNavigator jsonNavigator,
        ILogger<YahooFinancialsPageParser> logger)
    {
        _jsonNavigator = jsonNavigator;
        _logger = logger;
    }

    public AnnualReportDto? Parse(YahooPageContext pageContext)
    {
        try
        {
            var report = new AnnualReportDto();

            ApplyAnnualReportFromJson(pageContext.PayloadRoots, report);
            ApplyAnnualReportFromHtmlFallback(pageContext.Document, report);

            if (string.IsNullOrWhiteSpace(report.ReportedCurrency))
                report.ReportedCurrency = "USD";

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing annual report data for symbol {Symbol}", pageContext.Symbol);
            return null;
        }
    }

    private void ApplyAnnualReportFromJson(IReadOnlyList<JsonElement> payloadRoots, AnnualReportDto report)
    {
        var revenue = ReadLatestAnnualMetric(payloadRoots, "annualTotalRevenue");
        var costOfRevenue = ReadLatestAnnualMetric(payloadRoots, "annualReconciledCostOfRevenue", "annualCostOfRevenue");
        var grossProfit = ReadLatestAnnualMetric(payloadRoots, "annualGrossProfit");
        var operatingIncome = ReadLatestAnnualMetric(payloadRoots, "annualOperatingIncome");
        var netIncome = ReadLatestAnnualMetric(
            payloadRoots,
            "annualNetIncomeCommonStockholders",
            "annualNetIncome",
            "annualNetIncomeIncludingNoncontrollingInterests");
        var depreciation = ReadLatestAnnualMetric(
            payloadRoots,
            "annualReconciledDepreciation",
            "annualDepreciationAndAmortization");

        if (revenue is not null)
        {
            report.TotalRevenue = revenue.Value.Value;
            report.FiscalDateEnding = revenue.Value.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
            report.ReportedCurrency = revenue.Value.Currency;
        }

        if (costOfRevenue is not null)
            report.CostOfRevenue = costOfRevenue.Value.Value;

        if (grossProfit is not null)
            report.GrossProfit = grossProfit.Value.Value;

        if (operatingIncome is not null)
            report.OperatingIncome = operatingIncome.Value.Value;

        if (netIncome is not null)
            report.NetIncome = netIncome.Value.Value;

        if (depreciation is not null)
            report.DepreciationAndAmortization = depreciation.Value.Value;
    }

    private static void ApplyAnnualReportFromHtmlFallback(HtmlAgilityPack.HtmlDocument document, AnnualReportDto report)
    {
        if (string.IsNullOrWhiteSpace(report.ReportedCurrency))
        {
            var currencyNode = document.DocumentNode.SelectSingleNode("//span[contains(normalize-space(.), 'Currency in ')]");
            var currencyText = YahooHtmlValueReader.Clean(currencyNode?.InnerText);
            const string prefix = "Currency in ";

            if (currencyText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                report.ReportedCurrency = currencyText[prefix.Length..].Trim();
        }

        report.TotalRevenue = YahooValueFormatter.PreferExisting(report.TotalRevenue, YahooHtmlValueReader.GetFinancialStatementRowValue(document, "Total Revenue"));
        report.CostOfRevenue = YahooValueFormatter.PreferExisting(report.CostOfRevenue, YahooHtmlValueReader.GetFinancialStatementRowValue(document, "Cost of Revenue", "Reconciled Cost of Revenue"));
        report.GrossProfit = YahooValueFormatter.PreferExisting(report.GrossProfit, YahooHtmlValueReader.GetFinancialStatementRowValue(document, "Gross Profit"));
        report.OperatingIncome = YahooValueFormatter.PreferExisting(report.OperatingIncome, YahooHtmlValueReader.GetFinancialStatementRowValue(document, "Operating Income"));
        report.NetIncome = YahooValueFormatter.PreferExisting(
            report.NetIncome,
            YahooHtmlValueReader.GetFinancialStatementRowValue(
                document,
                "Net Income",
                "Net Income Common Stockholders",
                "Net Income Including Noncontrolling Interests",
                "Net Income from Continuing Operation Net Minority Interest"));
        report.DepreciationAndAmortization = YahooValueFormatter.PreferExisting(
            report.DepreciationAndAmortization,
            YahooHtmlValueReader.GetFinancialStatementRowValue(document, "Reconciled Depreciation", "Depreciation And Amortization"));
    }

    private AnnualMetric? ReadLatestAnnualMetric(IReadOnlyList<JsonElement> payloadRoots, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            AnnualMetric? latestMetric = null;

            foreach (var root in payloadRoots)
            {
                var metricArray = _jsonNavigator.FindArrayByPropertyName(root, propertyName);
                if (metricArray is null)
                    continue;

                foreach (var item in metricArray.Value.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object ||
                        !item.TryGetProperty("asOfDate", out var asOfDateNode) ||
                        !DateTime.TryParse(asOfDateNode.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                        !item.TryGetProperty("reportedValue", out var reportedValueNode) ||
                        reportedValueNode.ValueKind != JsonValueKind.Object ||
                        !reportedValueNode.TryGetProperty("fmt", out var valueNode))
                    {
                        continue;
                    }

                    var value = valueNode.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    var currency = item.TryGetProperty("currencyCode", out var currencyNode) && currencyNode.ValueKind == JsonValueKind.String
                        ? currencyNode.GetString() ?? string.Empty
                        : string.Empty;

                    if (latestMetric is null || date > latestMetric.Value.Date)
                        latestMetric = new AnnualMetric(date, currency, value);
                }
            }

            if (latestMetric is not null)
                return latestMetric;
        }

        return null;
    }

    private readonly record struct AnnualMetric(DateTime? Date, string Currency, string Value);

    private readonly IYahooJsonNavigator _jsonNavigator;
    private readonly ILogger<YahooFinancialsPageParser> _logger;
}

