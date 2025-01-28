// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestHosts
{
    public class YarpBffIntegrationTestBase
    {
        private readonly IdentityServerHost _identityServerHost;
        protected readonly ApiHost ApiHost;
        protected readonly YarpBffHost BffHost;
        private BffHostUsingResourceNamedTokens _bffHostWithNamedTokens;

        protected YarpBffIntegrationTestBase()
        {
            _identityServerHost = new IdentityServerHost();
            
            _identityServerHost.Clients.Add(new Client
            {
                ClientId = "spa",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RedirectUris = { "https://app/signin-oidc" },
                PostLogoutRedirectUris = { "https://app/signout-callback-oidc" },
                BackChannelLogoutUri = "https://app/bff/backchannel",
                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope1" }
            });
            
            
            _identityServerHost.OnConfigureServices += services => {
                services.AddTransient<IBackChannelLogoutHttpClient>(provider => 
                    new DefaultBackChannelLogoutHttpClient(
                        BffHost.HttpClient, 
                        provider.GetRequiredService<ILoggerFactory>(), 
                        provider.GetRequiredService<ICancellationTokenProvider>()));
            };
            
            _identityServerHost.InitializeAsync().Wait();

            ApiHost = new ApiHost(_identityServerHost, "scope1");
            ApiHost.InitializeAsync().Wait();

            BffHost = new YarpBffHost(_identityServerHost, ApiHost, "spa");
            BffHost.InitializeAsync().Wait();

            _bffHostWithNamedTokens = new BffHostUsingResourceNamedTokens(_identityServerHost, ApiHost, "spa");
            _bffHostWithNamedTokens.InitializeAsync().Wait();
        }

        public async Task Login(string sub)
        {
            await _identityServerHost.IssueSessionCookieAsync(new Claim("sub", sub));
        }
    }
}
