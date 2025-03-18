using System.Security.Claims;
using DTO;
using Inteerfaces;

namespace Bussines.UpdatePsw;

public class PasswordRecoveryService:IService
{
    IMail _mail;
    IRepository _repository;
    TokenRepository _tokenRepository;

    public PasswordRecoveryService(IMail mail, IRepository repository, TokenRepository tokenRepository)
    {
        _mail = mail;
        _repository = repository;
        _tokenRepository=tokenRepository;
    }

    public async Task<bool> GenerationTokenAsync(string email)
    {
        var user=await _repository.CheckExistByMail(email);
        if (!user) return false;

        var token = _tokenRepository.GeneratePasswordResetToken(email);
        return await _mail.SendToken(email, token);
    }

    public async Task<bool> CheckPassword(PasswordResetDto resetDto)
    {
        var principal = _tokenRepository.ValidateToken(resetDto.Token);
        if (principal == null) return false;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value; // Извлекаем значение email
        if (string.IsNullOrEmpty(email)) return false;

        var userID = await _repository.GetIdByEmail(email); // Дожидаемся результата запроса
        if (userID == null) return false;

        return await _repository.UpdateUserPassword(userID.Value, resetDto.NewPassword); // Передаём userID.Value
    }

}