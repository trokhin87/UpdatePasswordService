using System.Net.Http.Json;
using Inteerfaces;
using Microsoft.Extensions.Configuration;

namespace Bussines.UpdatePsw;

public class Repository:IRepository
{
    private readonly HttpClient _httpClient;
    private readonly string? _baseUrl;

    public Repository(HttpClient httpClient,IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ProxyMicroservice:BaseUrl"];
    }
    
    
    
    public async Task<Guid?> GetIdByEmail(string email)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/passwordrecovery/getid/{email}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        return null;
    }

    public async Task<bool> CheckExistByMail(string email)
    {
        var response=await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/passwordrecovery/check-mail",email);
        return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<bool> UpdateUserPassword(Guid Id, string newPassword)
    {
        var tmp=new{id=Id,password=newPassword};
        var response= await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/passwordrecovery/update",tmp);
        return response.IsSuccessStatusCode;
    }
}