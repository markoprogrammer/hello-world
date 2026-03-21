using HelloWorld.WebApi.Models;
using HelloWorld.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.WebApi.Controllers;

[ApiController]
[Route("oauth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// OAuth2 Token endpoint. Supports grant types: password, client_credentials, refresh_token.
    /// </summary>
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded", "application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Token([FromForm] TokenRequest request)
    {
        return request.GrantType switch
        {
            "password" => HandlePasswordGrant(request),
            "client_credentials" => HandleClientCredentialsGrant(request),
            "refresh_token" => HandleRefreshTokenGrant(request),
            _ => BadRequest(new ErrorResponse
            {
                Error = "unsupported_grant_type",
                ErrorDescription = $"Grant type '{request.GrantType}' is not supported. Use 'password', 'client_credentials', or 'refresh_token'."
            })
        };
    }

    private IActionResult HandlePasswordGrant(TokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Username and password are required for password grant."
            });
        }

        var user = _userService.ValidateCredentials(request.Username, request.Password);
        if (user is null)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "invalid_grant",
                ErrorDescription = "Invalid username or password."
            });
        }

        return Ok(_tokenService.GenerateTokenForUser(user));
    }

    private IActionResult HandleClientCredentialsGrant(TokenRequest request)
    {
        if (string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.ClientSecret))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Client ID and client secret are required for client_credentials grant."
            });
        }

        if (!_tokenService.ValidateClientCredentials(request.ClientId, request.ClientSecret))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "invalid_client",
                ErrorDescription = "Invalid client credentials."
            });
        }

        var scopes = _tokenService.GetClientScopes(request.ClientId) ?? [];
        return Ok(_tokenService.GenerateTokenForClient(request.ClientId, scopes));
    }

    private IActionResult HandleRefreshTokenGrant(TokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Refresh token is required."
            });
        }

        var response = _tokenService.RefreshAccessToken(request.RefreshToken);
        if (response is null)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "invalid_grant",
                ErrorDescription = "Invalid or expired refresh token."
            });
        }

        return Ok(response);
    }
}
