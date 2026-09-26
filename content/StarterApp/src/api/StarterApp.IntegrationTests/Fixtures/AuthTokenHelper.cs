#if (useLocalIdentity)
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using StarterApp.Application.Interfaces;
#endif

namespace StarterApp.IntegrationTests.Fixtures;

/// <summary>
/// Kept as a classic <c>this</c> extension method rather than a C# 14 extension block:
/// the .NET 10 compiler reports a spurious CS8620 nullability warning when a single
/// string is passed to a <c>params string[]</c> parameter declared inside an extension block.
/// </summary>
public static class AuthTokenHelper
{
#if (useLocalIdentity)
    public static async Task AuthenticateAsAsync(
        this HttpClient client, TestApiFactory factory, string id, params string[] roles)
    {
        using var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var token = await tokenService.CreateTokenAsync(new FakeIdentityUser
        {
            Id = id,
            Email = "admin@example.com",
            FullName = "Admin User",
            Roles = roles
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
#else
    /// <summary>
    /// Same signature as the JWT flavour so the tests themselves are identical: there is no
    /// token to sign, so the identity travels as headers for <see cref="TestAuthHandler"/>.
    /// </summary>
    public static Task AuthenticateAsAsync(
        this HttpClient client, TestApiFactory factory, string id, params string[] roles)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RolesHeader);

        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, id);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(',', roles));

        return Task.CompletedTask;
    }
#endif
}
