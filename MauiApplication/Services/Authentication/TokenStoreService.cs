using Corporate.Shared.Models.Returns.Data;
using Microsoft.Extensions.Logging;
using MauiApplication.Services.Authentication._interfaces;

using Microsoft.AspNetCore.Components.Authorization;

namespace MauiApplication.Services.Authentication;

public class TokenStoreService(ILogger<TokenStoreService> logger, IJwtService jwt, AuthStateProvider stateProvider) : ITokenStoreService
{
    private readonly ILogger<TokenStoreService> _logger = logger;
    private readonly IJwtService _jwt = jwt;
    private readonly AuthStateProvider _stateProvider = stateProvider;

    //===============================================//
    //         Реализация методов интерфейса         //
    //===============================================//

    public async Task<TokenPair> GetTokenPair()
    {
        var access = await SecureStorage.GetAsync("AccessToken");
        var refresh = await SecureStorage.GetAsync("RefreshToken");
        return new TokenPair { AccessToken = access, RefreshToken = refresh };
    }

    public Task<ObjectResultRecord> ClearStorage()
    {
        SecureStorage.Remove("AccessToken");
        SecureStorage.Remove("RefreshToken");
        return Task.FromResult(new ObjectResultRecord() { Message = "Токены удалены", Success = true });
    }


    public async Task WriteTokenToStore(TokenPair tokenData)
    {
        if (string.IsNullOrEmpty(tokenData.AccessToken))
        {
            throw new ArgumentNullException(tokenData.AccessToken);
        }
        await SecureStorage.SetAsync("AccessToken", tokenData.AccessToken);
        await SecureStorage.SetAsync("RefreshToken", tokenData.RefreshToken);
        _stateProvider.NotifyAuthChange();
    }

    public void RemoveTokenFromStore()
    {
        SecureStorage.Remove("RefreshToken");
        SecureStorage.Remove("AccessToken");
        _stateProvider.NotifyAuthChange();
    }
}