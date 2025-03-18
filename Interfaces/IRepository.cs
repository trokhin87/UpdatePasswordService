namespace Inteerfaces;

public interface IRepository
{
    Task<Guid?> GetIdByEmail(string email);
    Task<bool> CheckExistByMail(string email);
    Task<bool> UpdateUserPassword(Guid Id, string newPassword);
}