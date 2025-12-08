using Corporate.Shared.Models.Employees.Authentication;
using Corporate.Shared.Models.Returns.Data;
using MauiApplication.Services.Authentication._interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Monitor.Services._interfaces.Web;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;

namespace MauiApplication.Services.Web
{
    public class RequestService : IRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigation;
        private readonly ILogger<RequestService> _logger;
        private readonly ITokenStoreService _tokenStore;

        /// <summary>
        /// Базовый адрес сервера для запросов<br/>
        /// (Временно как hardcode. По плану будет браться из параметров приложения)
        /// </summary>
#if DEBUG
        private readonly Uri _baseUri = new Uri("http://192.168.2.126:8190");
#else
        private readonly Uri _baseUri = new Uri("http://193.107.233.74:8190");
#endif

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="clientFactory">Фабрика для создания http клиента</param>
        /// <param name="navigation"></param>
        /// <param name="logger"></param>
        /// <param name="tokenStore"></param>
        public RequestService(IHttpClientFactory clientFactory, NavigationManager navigation, ILogger<RequestService> logger, ITokenStoreService tokenStore)
        {
            _httpClient = clientFactory.CreateClient("ApiClient");
            _httpClient.BaseAddress = _baseUri;
            _navigation = navigation;
            _logger = logger;
            _tokenStore = tokenStore;
        }

        //=============================================//
        //         Реализация полей интерфейса         //
        //=============================================//

        /// <summary>
        /// Забираем стандартные заголовки из клиента
        /// </summary>
        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        //---------------------------------------------//
        public async Task<T> GetAsync<T>(string url, CancellationToken cToken)
        {
            var request = await RequestWithAuthorization(() => _httpClient.GetAsync(url, cToken));
            try
            {
                request.EnsureSuccessStatusCode();
                return await request.Content.ReadFromJsonAsync<T>(cToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Загрузка отменена");
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("\n\nНе удалось сериализовать токен. Возвращаем стандартное значение\n\nException: {Error}\n\n", ex.Message);
                return default(T);
            }

        }

        //---------------------------------------------//
        public async Task<HttpResponseMessage> PostAsync<T>(string url, T data, CancellationToken cToken)
        {
            var request = await RequestWithAuthorization(() => _httpClient.PostAsJsonAsync(url, data, cToken));
            request.EnsureSuccessStatusCode();
            return request;
        }

        //---------------------------------------------//
        public async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cToken)
        {
            var request = await RequestWithAuthorization(() => _httpClient.PostAsJsonAsync(url, data, cToken));
            request.EnsureSuccessStatusCode();
            return await request.Content.ReadFromJsonAsync<TResponse>(cToken);
        }

        //---------------------------------------------//

        public async Task<HttpResponseMessage> PatchAsync(string url, CancellationToken cToken)
        {
            var request = await RequestWithAuthorization(() => _httpClient.PatchAsync(url, null, cToken));
            request.EnsureSuccessStatusCode();
            return request;
        }

        //---------------------------------------------//

        public HubConnection GetSignalRObject(string apiPath)
        {
            try
            {
                var hubConnection = new HubConnectionBuilder().WithUrl(_baseUri + apiPath).Build();
                return hubConnection;
            }
            catch (Exception ex)
            {
                _logger.LogError("\n\n[{ErrorMethod}] - Ошибка создания объекта: {ErrorMessage} \n\n", MethodBase.GetCurrentMethod()?.Name, ex.Message);
                return null;
            }
        }

        //=========================================//
        //         Приватные методы класса         //
        //=========================================//


        /// <summary>
        /// Установка заголовков авторизации запросу
        /// </summary>
        private async Task AddAuthorization()
        {
            var token = await SecureStorage.GetAsync("AccessToken");
            if (token is not null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Обёртка для Http запросов <br />
        /// Выполняет обновление токенов при их не валидности. <br /><br />
        /// Проверяется статус запроса. Если Unauthorized 401, то выполняется refresh <br />
        /// заново устанавливаются заголовки авторизации и повторно выполняется запрос
        /// </summary>
        /// <param name="apiCall">Оборачиваемый таск</param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private async Task<HttpResponseMessage> RequestWithAuthorization(Func<Task<HttpResponseMessage>> apiCall)
        {
            try
            {
                await AddAuthorization();
                var request = await apiCall();
                if (request.StatusCode != HttpStatusCode.Unauthorized) return request;

                var newToken = await RefreshAsync();
                if (newToken is not null)
                {
                    await AddAuthorization();
                    return await apiCall();
                }

                await LogoutAsync();
                _navigation.NavigateTo("/login");
                throw new UnauthorizedAccessException("Токен доступа не валиден");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Загрузка отменена");
                throw new OperationCanceledException("Загрузка отменена");
            }
            catch (Exception ex)
            {
                _logger.LogError("\n\nОшибка при запросе с авторизацией: {ErrorMessage}\n\n", ex.Message);
                throw new Exception(ex.Message);
            }

        }

        /// <summary>
        /// Структура токенов для пере использования
        /// </summary>
        /// <param name="AccessToken">Токен доступа</param>
        /// <param name="RefreshToken">Токен обновления</param>
        private readonly record struct TokenResponse(string AccessToken, string RefreshToken);

        //=================================//
        //         Готовые запросы         //
        //=================================//

        /// <summary>
        /// Готовый запрос на аутентификацию
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ObjectResultRecord> LoginAsync(string login, string password)
        {
            var payload = new AuthorizationModel
            {
                Login = login,
                Password = password,
                Device = DeviceInfo.Platform == DevicePlatform.Android ? "Android" : "Windows",
                DeviceInfo = $"{DeviceInfo.Name} | {DeviceInfo.Manufacturer} | {DeviceInfo.Platform} | {DeviceInfo.Version}"
            };

            try
            {
                var request = await _httpClient.PostAsJsonAsync("api/auth/login", payload);
                if (!request.IsSuccessStatusCode) return new ObjectResultRecord { Message = "Вход не удался", Success = false };

                var tokenResponse = await request.Content.ReadFromJsonAsync<TokenResponse>();
                TokenPair tokeData = new(tokenResponse.AccessToken, tokenResponse.RefreshToken);
                // Запись полученных токенов в хранилище
                await _tokenStore.WriteTokenToStore(tokeData);

                return new ObjectResultRecord { Message = "Вход выполнен успешно", Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError("\n\nИсключение при логине:  {ExMessage}\n\n", ex.Message);
                return new ObjectResultRecord { Message = ex.Message, Success = false };
            }

        }

        /// <summary>
        /// Готовый запрос на обновление токенов
        /// </summary>
        /// <returns></returns>
        public async Task<string> RefreshAsync()
        {
            var refresh = await SecureStorage.GetAsync("RefreshToken");
            var requestData = new
            {
                RefreshToken = refresh,
                Device = DeviceInfo.Platform == DevicePlatform.Android ? "Android" : "Windows",
            };
            if (refresh is null) return null;

            try
            {
                var request = await _httpClient.PostAsJsonAsync("api/auth/refresh", requestData);
                if (!request.IsSuccessStatusCode) return null;

                var tokenResponse = await request.Content.ReadFromJsonAsync<TokenResponse>();
                TokenPair tokeData = new(tokenResponse.AccessToken, tokenResponse.RefreshToken);
                // Запись полученных токенов в хранилище
                await _tokenStore.WriteTokenToStore(tokeData);

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError("\n\nИсключение при обновлении токенов: {ErrorMessage}\n\n", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Готовый запрос на выход пользователя. <br />
        /// идёт запрос на уведомление сервера о выходе
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                var refresh = await SecureStorage.GetAsync("RefreshToken");
                var requestData = new
                {
                    RefreshToken = refresh,
                    Device = DeviceInfo.Platform == DevicePlatform.Android ? "Android" : "Windows",
                };

                if (refresh is not null)
                {
                    // Запрос в сервер побочным процессом, чтобы не ждать ответа
                    _ = Task.Run(async () =>
                    {
                        try { await _httpClient.PostAsJsonAsync("api/auth/logout", requestData); }
                        catch (Exception ex) { _logger.LogError("\n\nИсключение под запроса выхода пользователя: {ErrorMessage}\n\n", ex.Message); }
                    });
                }

                _tokenStore.RemoveTokenFromStore();
                _navigation.NavigateTo("/login");
            }
            catch (Exception ex)
            {
                _logger.LogError("\n\nИсключение при выходе пользователя: {ErrorMessage}\n\n", ex.Message);
            }
        }
    }
}
