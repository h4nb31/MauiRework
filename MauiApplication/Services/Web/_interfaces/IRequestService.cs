using System.Net.Http.Headers;
using Corporate.Shared.Models.Returns.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Monitor.Services._interfaces.Web;

public interface IRequestService
{
    /// <summary>
    /// Стандартные заголовки запроса http для изменения
    /// </summary>
    HttpRequestHeaders DefaultRequestHeaders { get; }

    /// <summary>
    /// Перегрузка GET с подставляемым типом вывода
    /// </summary>
    /// <param name="url">путь запроса</param>
    /// <param name="cToken">токен отмены</param>
    /// <typeparam name="T">кастомный принимаемый тип</typeparam>
    /// <returns></returns>
    Task<T> GetAsync<T>(string url, CancellationToken cToken);

    /// <summary>
    /// Стандартный POST запрос с токеном отмены
    /// </summary>
    /// <param name="url">путь запроса</param>
    /// <param name="data">данные для payload</param>
    /// <param name="cToken">токен отмены</param>
    /// <typeparam name="T">кастомный принимаемый тип</typeparam>
    /// <returns></returns>
    Task<HttpResponseMessage> PostAsync<T>(string url, T data, CancellationToken cToken);

    /// <summary>
    /// Перегрузка POST с Возвратом TResponse
    /// </summary>
    /// <param name="url">путь запроса</param>
    /// <param name="data">данные для payload</param>
    /// <param name="cToken">токен отмены</param>
    /// <typeparam name="TRequest">Тип запроса</typeparam>
    /// <typeparam name="TResponse">Тип ответа</typeparam>
    /// <returns></returns>
    Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cToken);

    /// <summary>
    /// Перегрузка PATCH без контента
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> PatchAsync(string url, CancellationToken cToken);

    /// <summary>
    /// Готовый запрос на аутентификацию
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<ObjectResultRecord> LoginAsync(string login, string password);

    /// <summary>
    /// Готовый запрос на обновление токенов
    /// </summary>
    /// <returns></returns>
    Task<string> RefreshAsync();

    /// <summary>
    /// Готовый запрос выхода пользователя <br />
    /// Запрос на сервер выполняется параллельным процессом без ожидания <br />
    /// Токены удаляются в любом случае
    /// </summary>
    /// <returns></returns>
    Task LogoutAsync();


    /// <summary>
    /// Создание и возвращение объекта hubConnection
    /// </summary>
    /// <param name="apiPath">путь до api хаба</param>
    /// <returns></returns>
    HubConnection GetSignalRObject(string apiPath);

}