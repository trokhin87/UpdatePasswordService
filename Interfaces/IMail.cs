namespace Inteerfaces;

public interface IMail
{
    Task<bool> SendToken(string email,string token);
}