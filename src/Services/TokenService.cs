using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HelloWorld.WebApi.Configuration;
using HelloWorld.WebApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace HelloWorld.WebApi.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ConcurrentDictionary<string, (string ClientSecret, List<string> Scopes)> _clients = new();
    private readonly ConcurrentDictionary<string, (string Username, DateTime Expiry)> _refreshTokens = new();

    public TokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public TokenResponse GenerateTokenForUser(UserRecord user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new("sub", user.Username),
            new("grant_type", "password")
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var accessToken = GenerateJwt(claims, TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpirationMinutes));
        var refreshToken = GenerateRefreshToken();

        _refreshTokens[refreshToken] = (user.Username, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            RefreshToken = refreshToken,
            Scope = string.Join(" ", user.Roles)
        };
    }

    public TokenResponse GenerateTokenForClient(string clientId, IEnumerable<string> scopes)
    {
        var scopeList = scopes.ToList();
        var claims = new List<Claim>
        {
            new("client_id", clientId),
            new("grant_type", "client_credentials")
        };

        foreach (var scope in scopeList)
        {
            claims.Add(new Claim("scope", scope));
        }

        var accessToken = GenerateJwt(claims, TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpirationMinutes));

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            Scope = string.Join(" ", scopeList)
        };
    }

    public TokenResponse? RefreshAccessToken(string refreshToken)
    {
        if (!_refreshTokens.TryRemove(refreshToken, out var tokenData))
            return null;

        if (tokenData.Expiry < DateTime.UtcNow)
            return null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, tokenData.Username),
            new("sub", tokenData.Username),
            new("grant_type", "refresh_token")
        };

        var accessToken = GenerateJwt(claims, TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpirationMinutes));
        var newRefreshToken = GenerateRefreshToken();

        _refreshTokens[newRefreshToken] = (tokenData.Username, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            RefreshToken = newRefreshToken
        };
    }

    public void RegisterClient(string clientId, string clientSecret, List<string> scopes)
    {
        _clients[clientId] = (clientSecret, scopes);
    }

    public bool ValidateClientCredentials(string clientId, string clientSecret)
    {
        return _clients.TryGetValue(clientId, out var client) && client.ClientSecret == clientSecret;
    }

    public List<string>? GetClientScopes(string clientId)
    {
        return _clients.TryGetValue(clientId, out var client) ? client.Scopes : null;
    }

    private string GenerateJwt(IEnumerable<Claim> claims, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
