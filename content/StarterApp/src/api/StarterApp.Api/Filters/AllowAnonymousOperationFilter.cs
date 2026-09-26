using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StarterApp.API.Filters;

/// <summary>
/// Swagger operation filter that removes the security requirement from endpoints
/// decorated with <see cref="AllowAnonymousAttribute"/>.
/// This ensures anonymous endpoints (login, register) appear unlocked in Swagger UI.
/// </summary>
public class AllowAnonymousOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo
            .DeclaringType?
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any() == true
            || context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

        if (hasAllowAnonymous)
        {
            operation.Security?.Clear();
        }
    }
}