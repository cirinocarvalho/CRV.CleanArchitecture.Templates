using Microsoft.AspNetCore.Mvc;
using StarterApp.API.Middleware;

namespace StarterApp.IntegrationTests.Fixtures;

/// <summary>
/// Test-only controller that throws specific exceptions for middleware integration testing.
/// Registered via AddApplicationPart in CustomWebApplicationFactory.
/// </summary>
[ApiController]
[Route("test-errors")]
public class TestErrorController : ControllerBase
{
    [HttpGet("unauthorized")]
    public IActionResult ThrowUnauthorized()
        => throw new UnauthorizedAccessException("Access denied");

    [HttpGet("bad-request")]
    public IActionResult ThrowBadRequest()
        => throw new InvalidOperationException("Invalid operation");

    [HttpGet("not-found")]
    public IActionResult ThrowNotFound()
        => throw new KeyNotFoundException("Resource not found");

    [HttpGet("validation")]
    public IActionResult ThrowValidation()
        => throw new ValidationException("Validation failed", new Dictionary<string, string[]>
        {
            { "Email", ["Invalid email format"] },
            { "Password", ["Password is too short"] }
        });

    [HttpGet("internal-error")]
    public IActionResult ThrowInternalError()
        => throw new Exception("An unexpected error occurred");
}
