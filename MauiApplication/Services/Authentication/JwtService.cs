using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MauiApplication.Services.Authentication._interfaces;
using Microsoft.Extensions.Logging;

namespace MauiApplication.Services.Authentication;

public class JwtService(ILogger<JwtService> logger) : IJwtService
{
    private readonly ILogger<JwtService> _logger = logger;
    public ClaimsPrincipal ParseToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if(jwtToken.ValidTo < DateTime.Now)
            {
                _logger.LogWarning("Токен доступа истёк");
                return null;
            }

            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError("\n\nИсключение при разложении токена: {ErrorMessage}\n\n", ex.Message);
            return null;
        }

    }
}