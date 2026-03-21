namespace HelloWorld.WebApi.Services;

public record UserRecord(string Username, string PasswordHash, List<string> Roles);

public interface IUserService
{
    UserRecord? ValidateCredentials(string username, string password);
    UserRecord? GetUser(string username);
    void CreateUser(string username, string password, List<string> roles);
}
