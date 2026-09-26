using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarterApp.Infrastructure.Identity;

namespace StarterApp.IntegrationTests.Fixtures;

/// <summary>
/// Marks a registered user active (registrations start inactive pending admin approval).
/// </summary>
public static class UserActivationHelper
{
    extension(TestApiFactory factory)
    {
        public async Task ActivateUserAsync(string email)
        {
            using var scope = factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                user.IsActive = true;
                await userManager.UpdateAsync(user);
            }
        }
    }
}
