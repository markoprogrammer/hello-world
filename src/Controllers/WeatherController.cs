using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.WebApi.Controllers;

/// <summary>
/// Example protected API controller demonstrating OAuth2 authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Get weather forecast (requires any valid token).
    /// </summary>
    [HttpGet("forecast")]
    public IActionResult GetForecast()
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });

        return Ok(forecast);
    }

    /// <summary>
    /// Admin-only endpoint (requires "admin" role).
    /// </summary>
    [HttpGet("admin-stats")]
    [Authorize(Roles = "admin")]
    public IActionResult GetAdminStats()
    {
        return Ok(new
        {
            TotalRequests = Random.Shared.Next(1000, 50000),
            ActiveUsers = Random.Shared.Next(10, 500),
            ServerUptime = $"{Random.Shared.Next(1, 365)} days"
        });
    }

    /// <summary>
    /// Get current user info from the JWT token.
    /// </summary>
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new
        {
            Identity = User.Identity?.Name,
            IsAuthenticated = User.Identity?.IsAuthenticated,
            Claims = claims
        });
    }
}
