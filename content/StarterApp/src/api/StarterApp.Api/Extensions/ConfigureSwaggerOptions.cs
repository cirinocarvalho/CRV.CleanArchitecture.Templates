using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using StarterApp.API.Filters;

namespace StarterApp.API.Extensions;

/// <summary>
/// Configures Swagger/OpenAPI documentation generation for API versioning.
/// Creates separate Swagger documentation for each API version with JWT Bearer support.
/// </summary>
public class ConfigureSwaggerOptions(
    IApiVersionDescriptionProvider provider
#if (useEntra)
    , IConfiguration configuration
#endif
    )
    : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures Swagger generation options for all API versions.
    /// </summary>
    /// <param name="options">Swagger generation options to configure</param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "StarterApp API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                    ? "This API version has been deprecated."
#if (useEntra)
                    : "StarterApp API — click Authorize and sign in with your organisational account."
#else
                    : "StarterApp API — use POST /api/v1/auth/login to get a JWT token, then click Authorize."
#endif
            });
        }
#if (useEntra)

        var instance = configuration["AzureAd:Instance"]?.TrimEnd('/');
        var tenantId = configuration["AzureAd:TenantId"];
        var apiScope = $"{configuration["AzureAd:Audience"]}/{configuration["AzureAd:Scopes"]}";

        // Authorization code with PKCE. The implicit flow is simpler to wire up but is
        // deprecated and returns the token in the URL fragment, where it lands in browser
        // history and referrer headers.
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Sign in with your organisational account.",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{instance}/{tenantId}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"{instance}/{tenantId}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        [apiScope] = "Access the API as the signed-in user"
                    }
                }
            }
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("oauth2", document),
                [apiScope]
            }
        });
#else

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Paste the JWT token returned by POST /api/v1/auth/login (no 'Bearer' prefix needed).",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                []
            }
        });
#endif

        options.OperationFilter<AllowAnonymousOperationFilter>();
    }
}