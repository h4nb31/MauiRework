using Corporate.Shared.Models.Returns.Data;

namespace MauiApplication.Services.Authentication._interfaces;

public interface ITokenStoreService
{
    /// <summary>
    /// Забрать пару токенов из хранилища
    /// </summary>
    /// <returns></returns>
    Task<TokenPair> GetTokenPair();

    /// <summary>
    /// Запись токенов в хранилище приложения
    /// </summary>
    /// <param name="tokenData"></param>
    /// <returns></returns>
    Task WriteTokenToStore(TokenPair tokenData);

    /// <summary>
    /// Удаление токенов из хранилища
    /// </summary>
    void RemoveTokenFromStore();

}