using System;
using System.Net.Http;
using System.Text.Json;
using Clients;
using Duende.IdentityModel.Client;

var clientId = Guid.NewGuid().ToString();
var clientSecret = Guid.NewGuid().ToString();

Console.Title = "Dynamic Client Registration - Client Credentials Flow";
await RegisterClient();
Console.ReadLine();
var response = await RequestTokenAsync();
response.Show();

Console.ReadLine();
await CallServiceAsync(response.AccessToken);
Console.ReadLine();

async Task RegisterClient()
{
    var client = new HttpClient();

    var request = new DynamicClientRegistrationRequest
    {
        Address = Constants.Authority + "/connect/dcr",
        Document = new DynamicClientRegistrationDocument
        {

            GrantTypes = { "client_credentials" },
            Scope = "resource1.scope1 resource2.scope1 IdentityServerApi"
        }
    };

    var json = JsonDocument.Parse(
        $$"""
        {
          "client_id": "{{clientId}}",
          "client_secret": "{{clientSecret}}"
        }
        """
    );

    var clientJson = json.RootElement.GetProperty("client_id");
    var secretJson = json.RootElement.GetProperty("client_secret");
    
    request.Document.Extensions!.Add("client_id", clientJson);
    request.Document.Extensions.Add("client_secret", secretJson);

    var response = await client.RegisterClientAsync(request);

    if (response.IsError)
    {
        Console.WriteLine(response.Error);
        return;
    }
    Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
}

async Task<TokenResponse> RequestTokenAsync()
{
    var client = new HttpClient();

    var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
    if (disco.IsError) throw new Exception(disco.Error);

    var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    {
        Address = disco.TokenEndpoint,

        ClientId = clientId,
        ClientSecret = clientSecret,
    });

    if (response.IsError)
    {
        Console.WriteLine("\n\nError:\n{0}", response.Error);
        Environment.Exit(-1);
        return null;
    }
    return response;
}

static async Task CallServiceAsync(string token)
{
    var baseAddress = Constants.SampleApi;

    var client = new HttpClient
    {
        BaseAddress = new Uri(baseAddress)
    };

    client.SetBearerToken(token);
    var response = await client.GetStringAsync("identity");

    "\n\nService claims:".ConsoleGreen();
    Console.WriteLine(response.PrettyPrintJson());
}