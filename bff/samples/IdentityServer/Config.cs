// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer.Models;

namespace IdentityServerHost
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            [
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            ];

        public static IEnumerable<ApiScope> ApiScopes =>
            [
                new ApiScope("api", ["name"]),
                new ApiScope("scope-for-isolated-api", ["name"]),
            ];

        public static IEnumerable<ApiResource> ApiResources =>
            [
                new ApiResource("urn:isolated-api", "isolated api")
                {
                    RequireResourceIndicator = true,
                    Scopes = { "scope-for-isolated-api" }
                }
            ];


    }
}