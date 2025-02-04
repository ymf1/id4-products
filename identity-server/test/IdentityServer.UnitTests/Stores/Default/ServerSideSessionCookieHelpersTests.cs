// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Stores.Default;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using UnitTests.Common;

namespace UnitTests.Stores.Default;

public class ServerSideSessionCookieHelpersTests
{
    [Fact]
    public async Task OnCheckSlidingExpiration_if_force_renewal_cookie_flag_not_present_does_not_change_should_renew()
    {
        var context = CreateSlidingExpirationContext();
        
        await ServerSideSessionCookieHelpers.OnCheckSlidingExpiration(context);

        context.ShouldRenew.Should().BeFalse();
    }
    
    [Fact]
    public async Task OnCheckSlidingExpiration_if_force_renewal_cookie_flag_present_sets_should_renew_true()
    {
        var context = CreateSlidingExpirationContext(true);
        
        await ServerSideSessionCookieHelpers.OnCheckSlidingExpiration(context);

        context.ShouldRenew.Should().BeTrue();
    }

    [Fact]
    public async Task OnCheckSlidingExpiration_if_force_renewal_cookie_flag_present_removes_flag()
    {
        var context = CreateSlidingExpirationContext(true);
        
        await ServerSideSessionCookieHelpers.OnCheckSlidingExpiration(context);

        context.Properties.Items.Keys.Should().NotContain(IdentityServerConstants.ForceCookieRenewalFlag);
    }

    private CookieSlidingExpirationContext CreateSlidingExpirationContext(bool forceRenew = false)
    {
        var httpContext = new MockHttpContextAccessor().HttpContext;
        var authScheme = new AuthenticationScheme("Test", "Test", typeof(MockAuthenticationHandler));
        var authOptions = new CookieAuthenticationOptions();
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true
        };
        if (forceRenew)
        {
            authProperties.SetString(IdentityServerConstants.ForceCookieRenewalFlag, String.Empty);
        }
        
        var authTicket = new AuthenticationTicket(new ClaimsPrincipal([]), authProperties, "Test");
        var context = new CookieSlidingExpirationContext(httpContext!, authScheme, authOptions, authTicket,
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15))
        {
            ShouldRenew = false
        };

        return context;
    }
}