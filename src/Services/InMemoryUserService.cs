using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace HelloWorld.WebApi.Services;

public class InMemoryUserService : IUserService
{
    private readonly ConcurrentDictionary<string, UserRecord> _users = new();

    public void CreateUser(string username, string password, List<string> roles)
    {
        var hash = HashPassword(password);
        _users[username] = new UserRecord(username, hash, roles);
    }

    public UserRecord? ValidateCredentials(string username, string password)
    {
        if (!_users.TryGetValue(username, out var user))
            return null;

        return HashPassword(password) == user.PasswordHash ? user : null;
    }

    public UserRecord? GetUser(string username)
    {
        _users.TryGetValue(username, out var user);
        return user;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
