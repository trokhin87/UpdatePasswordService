namespace Inteerfaces;

public interface IService
{
    Task<bool> GenerationTokenAsync(string email);
    Task<bool> CheckPassword();
}