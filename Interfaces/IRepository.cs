namespace Inteerfaces;

public interface IRepository
{
    Task<Guid?> GetIdByEmail(string email);
    Task<bool> CheckExistByMail(string email);
    Task<bool> UpdateUserPassword(string login, string newPassword);
    Task<string?> GetLoginByMail(string email);
}