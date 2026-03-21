using System.ComponentModel.DataAnnotations;

namespace HelloWorld.WebApi.Models;

public class TokenRequest
{
    /// <summary>
    /// OAuth2 grant type: "password" or "client_credentials"
    /// </summary>
    [Required]
    public string GrantType { get; set; } = string.Empty;

    // For "password" grant
    public string? Username { get; set; }
    public string? Password { get; set; }

    // For "client_credentials" grant
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    // For "refresh_token" grant
    public string? RefreshToken { get; set; }
}
