#if (useLocalIdentity)
using System.Text;
#endif
#if (useAuth)
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
#endif
#if (useEntra)
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
#endif
#if (useLocalIdentity)
using Microsoft.IdentityModel.Tokens;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;
using StarterApp.Infrastructure.Data.Contexts;
using StarterApp.Infrastructure.Data;
using StarterApp.Infrastructure.Data.Repositories;
#if (useAuth)
using StarterApp.Infrastructure.Identity;
#endif
using StarterApp.Infrastructure.Logging;
using StarterApp.Infrastructure.Services;

namespace StarterApp.Infrastructure;

/// <summary>
/// Dependency injection extension members for the Infrastructure layer.
/// Configures database access, authentication, and external services.
/// </summary>
public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds infrastructure layer services to the dependency injection container.
        /// Configures database contexts, authentication, identity, and external services.
        /// </summary>
        /// <param name="configuration">Application configuration for connection strings and settings</param>
        /// <returns>The configured service collection for method chaining</returns>
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            // --- Database contexts ---
#if (useAuth)
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("IdentityConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

#endif
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("AppConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "public")));
#if (useAuth)

            // --- Identity ---
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

#if (useLocalIdentity)
            // --- JWT ---
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName);
            services.Configure<JwtSettings>(jwtSettings);

            // Fail fast with a clear message: an empty or short key throws deep inside the JWT
            // handler on the first authenticated request, which is far harder to diagnose.
            var secret = jwtSettings.Get<JwtSettings>()?.Secret;
            if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            {
                throw new InvalidOperationException(
                    $"'{JwtSettings.SectionName}:Secret' must be set to at least 32 bytes. " +
                    "Set it with user-secrets or an environment variable, for example: " +
                    $"dotnet user-secrets set \"{JwtSettings.SectionName}:Secret\" \"<32+ character key>\"");
            }

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
#endif
#if (useEntra)
            // --- Microsoft Entra ID ---
            // Entra issues the token, so the API validates rather than mints one.
            //
            // Both properties are set explicitly, not just DefaultScheme: AddIdentity above
            // assigns DefaultAuthenticateScheme and DefaultChallengeScheme to its cookie
            // scheme, and DefaultScheme alone does not displace them — every bearer request
            // would be handed to the cookie handler and rejected as 401.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

            // Bridges the Entra token to the local store: resolves (or creates) the matching
            // ApplicationUser and republishes the same claims the JWT flavour issues, so every
            // controller, [Authorize(Roles = ...)] check and service works unchanged.
            services.AddScoped<IClaimsTransformation, EntraClaimsTransformation>();
#endif

            services.AddAuthorization();

            // --- Identity services ---
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IRoleManagerService, RoleManagerService>();
#if (useLocalIdentity)
            services.AddScoped<ITokenService, JwtTokenService>();
#endif
#endif

            // --- Logging adapter ---
            services.AddScoped(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

            // --- File storage ---
            services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
            services.AddScoped<IFileStorageService, FileStorageService>();

            // --- Email ---
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            services.AddScoped<IEmailService, EmailService>();
#if (useAuth)
            services.AddScoped<IAccountEmailSender, AccountEmailSender>();
#endif

            // --- Unit of Work ---
            services.AddScoped<IUnitOfWork>(sp => new UnitOfWork<AppDbContext>(sp.GetRequiredService<AppDbContext>()));

            // --- Repositories (generic) ---
#if (useAuth)
            services.AddScoped<IRepositoryBase<Company>, RepositoryBase<Company, AppDbContext>>();
            services.AddScoped<IReadRepositoryBase<Company>, ReadRepositoryBase<Company, AppDbContext>>();
            services.AddScoped<IRepositoryBase<UserCompany>, RepositoryBase<UserCompany, AppDbContext>>();
            services.AddScoped<IReadRepositoryBase<UserCompany>, ReadRepositoryBase<UserCompany, AppDbContext>>();
#endif
            services.AddScoped<IRepositoryBase<FileUploadDetail>, RepositoryBase<FileUploadDetail, AppDbContext>>();
#if (includeSamples)
            services.AddScoped<IRepositoryBase<Product>, RepositoryBase<Product, AppDbContext>>();
            services.AddScoped<IReadRepositoryBase<Product>, ReadRepositoryBase<Product, AppDbContext>>();
#endif

            return services;
        }
    }
}
