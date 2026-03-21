using HelloWorld.WebApi.Models;

namespace HelloWorld.WebApi.Services;

public interface ITokenService
{
    TokenResponse GenerateTokenForUser(UserRecord user);
    TokenResponse GenerateTokenForClient(string clientId, IEnumerable<string> scopes);
    TokenResponse? RefreshAccessToken(string refreshToken);
    void RegisterClient(string clientId, string clientSecret, List<string> scopes);
    bool ValidateClientCredentials(string clientId, string clientSecret);
    List<string>? GetClientScopes(string clientId);
}
