using System.Net.Http.Json;
using DTO;
using Inteerfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bussines.UpdatePsw;

public class Repository : IRepository
{
    private readonly HttpClient _httpClient;
    private readonly string? _baseUrl;
    private readonly ILogger<Repository> _logger;

    public Repository(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<Repository> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _baseUrl = configuration["ProxyMicroservice:BaseUrl"];
        _logger = logger;
    }

    public async Task<Guid?> GetIdByEmail(string email)
    {
        try
        {
            _logger.LogInformation("Запрос ID пользователя по email: {Email}", email);
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/passwordrecovery/getid/{email}");

            if (response.IsSuccessStatusCode)
            {
                var userId = await response.Content.ReadFromJsonAsync<Guid>();
                _logger.LogInformation("ID пользователя найден: {UserId}", userId);
                return userId;
            }

            _logger.LogWarning("Не удалось получить ID пользователя. Код ответа: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе ID пользователя по email: {Email}", email);
            return null;
        }
    }
    public async Task<string?> GetLoginByMail(string email)
    {
        try
        {
            _logger.LogInformation("Запрос ID пользователя по email: {Email}", email);
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/passwordrecovery/getlogin/{email}");

            if (response.IsSuccessStatusCode)
            {
                var login = await response.Content.ReadFromJsonAsync<string?>();
                _logger.LogInformation("login пользователя найден: {login}", login);
                return login;
            }

            _logger.LogWarning("Не удалось получить ID пользователя. Код ответа: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе login пользователя по email: {Email}", email);
            return null;
        }
    }

    public async Task<bool> CheckExistByMail(string email)
    {
        try
        {
            _logger.LogInformation("Проверка существования пользователя с email: {Email}", email);
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/passwordrecovery/check-mail", email);

            if (response.IsSuccessStatusCode)
            {
                bool exists = await response.Content.ReadFromJsonAsync<bool>();
                _logger.LogInformation("Пользователь {Email} существует: {Exists}", email, exists);
                return exists;
            }

            _logger.LogWarning("Не удалось проверить существование пользователя {Email}. Код ответа: {StatusCode}", email, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования пользователя {Email}", email);
            return false;
        }
    }

    public async Task<bool> UpdateUserPassword(string login, string newPassword)
    {
        try
        {
            _logger.LogInformation("Обновление пароля для пользователя {UserId}", login);
            LoginDto dto = new LoginDto { Login = login, Password = newPassword };
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/passwordrecovery/update", dto);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Пароль пользователя {UserId} успешно обновлен", login);
                return true;
            }

            _logger.LogWarning("Ошибка обновления пароля пользователя {UserId}. Код ответа: {StatusCode}", login, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пароля пользователя {UserId}", login);
            return false;
        }
    }
}
