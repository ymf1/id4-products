// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Microsoft.Extensions.ServiceDiscovery;

namespace IdentityServer;

/// <summary>
/// This client store will register a list of hard coded clients but will use
/// service discovery to ask for the correct urls.
///
/// This is needed because the actually used url's need to be set in Identity Server. 
/// </summary>
/// <param name="resolver"></param>
public class ServiceDiscoveringClientStore(ServiceEndpointResolver resolver) : IClientStore
{
    private List<Client> _clients = null;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private async Task Initialize()
    {

        await _semaphore.WaitAsync();
        try
        {


            if (_clients != null)
            {
                return;
            }
            // Get the BFF URL from the service discovery system. Then use this for building the redirect urls etc..
            var bffUrl = (await resolver.GetEndpointsAsync("https://bff", CancellationToken.None)).Endpoints.First().EndPoint.ToString();

            _clients = [
                new Client
                {
                    ClientId = "bff",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes =
                    {
                        GrantType.AuthorizationCode,
                        GrantType.ClientCredentials,
                        OidcConstants.GrantTypes.TokenExchange
                    },

                    RedirectUris = { $"{bffUrl}signin-oidc" },
                    FrontChannelLogoutUri = $"{bffUrl}signout-oidc",
                    PostLogoutRedirectUris = { $"{bffUrl}signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api", "scope-for-isolated-api" },

                    AccessTokenLifetime = 75 // Force refresh
                },
                new Client
                {
                    ClientId = "bff.dpop",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequireDPoP = true,

                    AllowedGrantTypes =
                    {
                        GrantType.AuthorizationCode,
                        GrantType.ClientCredentials,
                        OidcConstants.GrantTypes.TokenExchange
                    },

                    RedirectUris = { "https://localhost:5003/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:5003/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api", "scope-for-isolated-api" },

                    AccessTokenLifetime = 75 // Force refresh
                },
                new Client
                {
                    ClientId = "bff.ef",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes =
                    {
                        GrantType.AuthorizationCode,
                        GrantType.ClientCredentials,
                        OidcConstants.GrantTypes.TokenExchange
                    },
                    RedirectUris = { "https://localhost:5004/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:5004/signout-oidc",
                    BackChannelLogoutUri = "https://localhost:5004/bff/backchannel",
                    PostLogoutRedirectUris = { "https://localhost:5004/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api", "scope-for-isolated-api" },

                    AccessTokenLifetime = 75 // Force refresh
                },

                new Client
                {
                    ClientId = "blazor",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes =
                    {
                        GrantType.AuthorizationCode,
                        GrantType.ClientCredentials,
                        OidcConstants.GrantTypes.TokenExchange
                    },

                    RedirectUris = { "https://localhost:5005/signin-oidc", "https://localhost:5105/signin-oidc" },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:5005/signout-callback-oidc", "https://localhost:5105/signout-callback-oidc"
                    },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api", "scope-for-isolated-api" },

                    AccessTokenLifetime = 75
                }
            ];
        }
        finally
        {
            _semaphore.Release();
        }

    }


    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        await Initialize();
        return _clients?.FirstOrDefault(x => x.ClientId == clientId);
    }
}