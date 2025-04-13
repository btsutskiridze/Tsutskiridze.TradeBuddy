using HtmlAgilityPack;
using System.Net;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo.Utilities
{
    public class YahooCookieBypassService : IYahooCookieBypassService
    {
        /// <summary>
        /// Loads the HTML content from the given URL and, if a cookie consent form is found,
        /// bypasses it by submitting the form.
        /// </summary>
        public async Task<string> GetHtmlContentWithCookieBypass(HttpClient client, string url, ILogger logger)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();

            try
            {
                // Try to extract and submit the form.
                var formData = ExtractFormData(html);
                html = await SubmitFormAsync(formData);
                logger.LogDebug("Yahoo cookie bypass successful for URL {Url}", url);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "No cookie bypass needed or failed for URL {Url}", url);
            }

            return html;
        }

        private Dictionary<string, string> ExtractFormData(string html)
        {
            var formData = new Dictionary<string, string>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Locate the consent form (using a class selector, for example)
            var form = doc.DocumentNode.SelectSingleNode("//form[contains(@class, 'consent-form')]");
            if (form == null)
            {
                throw new Exception("Consent form not found.");
            }

            // Get the action attribute from the form.
            var action = form.GetAttributeValue("action", string.Empty);
            formData["__formAction"] = string.IsNullOrEmpty(action) ? "https://uk.yahoo.com/" : action;

            // Extract all input fields from the form.
            var inputs = form.SelectNodes(".//input");
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    var name = input.GetAttributeValue("name", string.Empty);
                    var value = input.GetAttributeValue("value", string.Empty);
                    if (!string.IsNullOrEmpty(name))
                    {
                        formData[name] = value;
                    }
                }
            }

            // Simulate the "accept" click if necessary.
            if (!formData.ContainsKey("agree"))
            {
                formData["agree"] = "agree";
            }

            return formData;
        }

        private async Task<string> SubmitFormAsync(Dictionary<string, string> formData)
        {
            // Retrieve the action URL (here we assume sessionId is part of formData)
            string actionUrl = "https://consent.yahoo.com/v2/collectConsent?sessionid=" + formData["sessionId"];
            formData.Remove("__formAction");

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                var content = new FormUrlEncodedContent(formData);
                var response = await client.PostAsync(actionUrl, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}