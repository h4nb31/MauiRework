using System.Security.Claims;
using MauiApplication.Services.Authentication._interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;


namespace MauiApplication.Services.Authentication;

/// <summary>
/// Этот класс через Scoped сервис передаёт в компоненты состояние пользователя <br/>
/// И уведомляет их об его изменении
/// </summary>
/// <param name="jwt"></param>
/// <param name="logger"></param>
public class AuthStateProvider(IJwtService jwt, ILogger<AuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private readonly IJwtService _jwt = jwt;
    private readonly ILogger<AuthenticationStateProvider> _logger = logger;

    /// <summary>
    /// Переопределение получения состояния пользователя
    /// </summary>
    /// <returns></returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var accessToken = await SecureStorage.GetAsync("AccessToken");
            if (string.IsNullOrEmpty(accessToken)) return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var user = _jwt.ParseToken(accessToken);
            return user != null ? new AuthenticationState(user) : new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
        {
            _logger.LogError("\n\nОшибка определения состояния аутентификации: {ErrorMessage}\n\n", ex.Message);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Метод для уведомления об изменении состояния пользователя
    /// </summary>
    public void NotifyAuthChange()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

}