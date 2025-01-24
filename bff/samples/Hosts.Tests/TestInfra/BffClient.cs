// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Web;
using HtmlAgilityPack;
using Shouldly;

namespace Hosts.Tests.TestInfra;

/// <summary>
/// Client for the BFF. All the methods that can be invoked are here. 
/// </summary>
public class BffClient
{
    private readonly HttpClient _client;

    public BffClient(HttpClient client)
    {
        _client = client;

        // Add a header that will trigger pre-flight cors checks
        _client.DefaultRequestHeaders.Add("X-CSRF", "1");
    }

    public async Task TriggerLogin(string userName = "alice", string password = "alice", CancellationToken ct = default)
    {
        var triggerLoginResponse = await _client.GetAsync("/bff/login");

        triggerLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loginPage = triggerLoginResponse.RequestMessage?.RequestUri ?? throw new InvalidOperationException("Can't find the login page.");
        loginPage.AbsolutePath.ShouldBe("/Account/Login");

        var html = await triggerLoginResponse.Content.ReadAsStringAsync();
        var form = ExtractForm(html);

        form.Fields["Input.Username"] = "alice";
        form.Fields["Input.Password"] = "alice";
        form.Fields["Input.Button"] = "login";

        var postLoginResponse = await _client.PostAsync(new Uri(loginPage, form.FormUrl), new FormUrlEncodedContent(form.Fields), ct);

        postLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        postLoginResponse.RequestMessage?.RequestUri?.Authority.ShouldBe(_client.BaseAddress?.Authority, await postLoginResponse.Content.ReadAsStringAsync(ct));
    }


    /// Parses the HTML content and extracts all form fields into a dictionary.
    /// </summary>
    /// <param name="htmlContent">The HTML content to parse.</param>
    /// <returns>A dictionary where the keys are field names and the values are their default values.</returns>
    private Form ExtractForm(string htmlContent)
    {
        var formFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Load the HTML content
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var form = htmlDoc.DocumentNode.SelectSingleNode("//form");

        // Select all input elements
        var inputNodes = form.SelectNodes("//input");
        if (inputNodes != null)
        {
            foreach (var inputNode in inputNodes)
            {
                var name = inputNode.GetAttributeValue("name", null);
                var value = inputNode.GetAttributeValue("value", string.Empty);

                if (!string.IsNullOrEmpty(name))
                {

                    formFields[name] = HttpUtility.HtmlDecode(value);
                }
            }
        }

        return new Form()
        {
            Fields = formFields,
            FormUrl = form.Attributes["action"].Value
        };
    }

    public async Task TriggerLogout()
    {
        // To trigger a logout, we need the logout claim
        var userClaims = await GetUserClaims();

        var logoutLink = userClaims.FirstOrDefault(x => x.Type == "bff:logout_url")
                         ?? throw new InvalidOperationException("Failed to find logout link claim");

        var logoutResponse = await _client.GetAsync(logoutLink.Value.ToString());
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    public async Task<UserClaim[]> GetUserClaims()
    {
        var userClaimsString = await _client.GetStringAsync("/bff/user");
        var userClaims = JsonSerializer.Deserialize<UserClaim[]>(userClaimsString, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        })!;
        return userClaims;
    }

    public async Task InvokeApi(string url, HttpStatusCode expectedResponse = HttpStatusCode.OK)
    {
        var response = await _client.GetAsync(url);

        response.StatusCode.ShouldBe(expectedResponse);
    }

    public record UserClaim
    {
        public required string Type { get; init; }
        public required object Value { get; init; }
    }

    private record Form
    {
        public required string FormUrl { get; init; }
        public required Dictionary<string, string> Fields { get; init; }
    }


}