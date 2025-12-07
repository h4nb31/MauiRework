using System.Security.Claims;

namespace MauiApplication.Services.Authentication._interfaces;

public interface IJwtService
{
    /// <summary>
    /// Разбираем токен на идентификатор и клеймы для получения информации оп пользователе
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    ClaimsPrincipal ParseToken(string token);
}