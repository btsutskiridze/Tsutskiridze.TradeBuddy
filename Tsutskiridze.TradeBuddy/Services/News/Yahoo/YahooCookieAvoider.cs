using HtmlAgilityPack;
using System.Net;

namespace Tsutskiridze.TradeBuddy.Services.News.Yahoo
{
    public class YahooCookieAvoider
    {
        public static Dictionary<string, string> ExtractFormData(string html)
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
            // If empty, you'll need to use a default URL (the same page URL)
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

            // Check for the Accept button. Since it’s a <button> element,
            // it might not have been captured in the input collection.
            // Here we simulate the "click" by adding the "agree" field.
            if (!formData.ContainsKey("agree"))
            {
                formData["agree"] = "agree";
            }

            return formData;
        }


        public static async Task<string> SubmitFormAsync(Dictionary<string, string> formData)
        {
            // Retrieve the action URL (remove the temporary key)
            string actionUrl = "https://consent.yahoo.com/v2/collectConsent?sessionid=" + formData["sessionId"]; //formData["__formAction"];
            formData.Remove("__formAction");

            // Create an HttpClient with a cookie container (if needed)
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using (var client = new HttpClient(handler))
            {
                // (Optional) Add any needed headers like User-Agent or Referer
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                // Create the POST content with the form data
                var content = new FormUrlEncodedContent(formData);

                // Submit the form
                var response = await client.PostAsync(actionUrl, content);

                // Ensure success or handle errors
                response.EnsureSuccessStatusCode();

                // Get the response content (which should be the page after accepting cookies)
                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
        }
    }
}
