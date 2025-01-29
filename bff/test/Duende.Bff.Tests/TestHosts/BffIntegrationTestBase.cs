// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Duende.Bff.Tests.TestHosts
{
    public class BffIntegrationTestBase
    {
        protected readonly IdentityServerHost IdentityServerHost;
        protected ApiHost ApiHost;
        protected BffHost BffHost;
        protected BffHostUsingResourceNamedTokens BffHostWithNamedTokens;

        public BffIntegrationTestBase(ITestOutputHelper output)
        {
            IdentityServerHost = new IdentityServerHost(output.WriteLine);
            
            IdentityServerHost.Clients.Add(new Client
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
            
            
            IdentityServerHost.OnConfigureServices += services => {
                services.AddTransient<IBackChannelLogoutHttpClient>(provider => 
                    new DefaultBackChannelLogoutHttpClient(
                        BffHost.HttpClient, 
                        provider.GetRequiredService<ILoggerFactory>(), 
                        provider.GetRequiredService<ICancellationTokenProvider>()));

                services.AddSingleton<DefaultAccessTokenRetriever>();
            };
            
            IdentityServerHost.InitializeAsync().Wait();

            ApiHost = new ApiHost(output.WriteLine, IdentityServerHost, "scope1");
            ApiHost.InitializeAsync().Wait();

            BffHost = new BffHost(output.WriteLine, IdentityServerHost, ApiHost, "spa");
            BffHost.InitializeAsync().Wait();

            BffHostWithNamedTokens = new BffHostUsingResourceNamedTokens(output.WriteLine, IdentityServerHost, ApiHost, "spa");
            BffHostWithNamedTokens.InitializeAsync().Wait();
        }

        public async Task Login(string sub)
        {
            await IdentityServerHost.IssueSessionCookieAsync(new Claim("sub", sub));
        }
    }
}
