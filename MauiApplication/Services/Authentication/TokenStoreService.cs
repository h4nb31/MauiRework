using Corporate.Shared.Models.Returns.Data;
using Microsoft.Extensions.Logging;
using MauiApplication.Services.Authentication._interfaces;

namespace MauiApplication.Services.Authentication;

public class TokenStoreService : ITokenStoreService
{
    private readonly ILogger<TokenStoreService> _logger;
    private readonly IJwtService _jwt;
    private AuthStateProvider _authState;

    public TokenStoreService(ILogger<TokenStoreService> logger, IJwtService jwt, AuthStateProvider authState)
    {
        _logger = logger;
        _jwt = jwt;
        _authState = authState;
    }

    //===============================================//
    //         Реализация методов интерфейса         //
    //===============================================//

    public async Task<TokenPair> GetTokenPair()
    {
        var access = await SecureStorage.GetAsync("AccessToken");
        var refresh = await SecureStorage.GetAsync("RefreshToken");
        return new TokenPair { AccessToken = access, RefreshToken = refresh };
    }

    public async Task WriteTokenToStore(TokenPair tokenData)
    {
        if (string.IsNullOrEmpty(tokenData.AccessToken))
        {
            throw new ArgumentNullException(tokenData.AccessToken);
        }
        await SecureStorage.SetAsync("AccessToken", tokenData.AccessToken);
        await SecureStorage.SetAsync("RefreshToken", tokenData.RefreshToken);
        _authState.NotifyAuthChange();
    }

    public void RemoveTokenFromStore()
    {
        SecureStorage.Remove("RefreshToken");
        SecureStorage.Remove("AccessToken");
        _authState.NotifyAuthChange();
    }
}