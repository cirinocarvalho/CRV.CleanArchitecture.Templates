using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using StarterApp.API.Extensions;
using StarterApp.API.Middleware;
using StarterApp.Application;
using StarterApp.Infrastructure;
using StarterApp.Infrastructure.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add structured logging (Serilog)
builder.AddStructuredLogging();

builder.Services.AddControllers(options =>
{
    // Treat non-nullable reference-type properties as optional so a missing field is a
    // validation concern for the model to express, not an automatic 400.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// --- CORS ---
var corsSettings = builder.Configuration
    .GetSection(CorsSettings.SectionName)
    .Get<CorsSettings>() ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

#if (useLocalIdentity)
// --- Rate limiting (anonymous auth endpoints) ---
builder.Services.AddAuthRateLimiting(builder.Configuration);

#endif
// --- Health checks (liveness + database readiness) ---
builder.Services.AddAppHealthChecks();

// --- API Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// --- Swagger per version ---
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

// Clean architecture DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Default the upload folder to App_Data\uploads under the app content root when it
// isn't explicitly configured (FileStorage:RootFolder).
builder.Services.PostConfigure<FileStorageSettings>(settings =>
{
    if (string.IsNullOrWhiteSpace(settings.RootFolder))
        settings.RootFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"StarterApp API {description.GroupName.ToUpperInvariant()}");
        }

#if (useEntra)
        // Swagger UI is a public client, so it needs its own app registration with
        // /swagger/oauth2-redirect.html as a redirect URI. Set AzureAd:SwaggerClientId when
        // that is a separate registration from the API itself.
        options.OAuthClientId(
            builder.Configuration["AzureAd:SwaggerClientId"]
            ?? builder.Configuration["AzureAd:ClientId"]);
        options.OAuthUsePkce();
        options.OAuthScopeSeparator(" ");
#endif
        options.EnablePersistAuthorization();
    });
}

// Exception handling middleware - must be first in the pipeline
app.UseGlobalExceptionHandler();

// Security logging middleware - logs security events
app.UseSecurityLogging();

// Security middleware - must be early in the pipeline
app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
#if (useLocalIdentity)

// After UseCors so CORS preflight is answered without consuming a permit, and
// before authentication so rejected callers cost no credential validation work.
app.UseRateLimiter();
#endif
#if (useAuth)

app.UseAuthentication();
app.UseAuthorization();
#endif
app.MapControllers();
app.MapAppHealthChecks();

app.Run();
