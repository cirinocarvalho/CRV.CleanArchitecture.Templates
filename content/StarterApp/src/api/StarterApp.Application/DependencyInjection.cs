using Microsoft.Extensions.DependencyInjection;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Services;

namespace StarterApp.Application;

/// <summary>
/// Dependency injection extension members for the Application layer.
/// Registers application services (use cases) with the DI container.
/// </summary>
public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds application layer services to the dependency injection container.
        /// Registers use case implementations with their corresponding interfaces.
        /// </summary>
        /// <returns>The configured service collection for method chaining</returns>
        public IServiceCollection AddApplication()
        {
#if (useLocalIdentity)
            services.AddScoped<IUserService, AuthService>();
#endif
#if (useAuth)
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IUserAdminService, UserAdminService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyAdminService, CompanyAdminService>();
#endif
            services.AddScoped<IFileUploadService, FileUploadService>();
#if (includeSamples)
            services.AddScoped<IProductService, ProductService>();
#endif
            return services;
        }
    }
}
