namespace Bussines.UpdatePsw;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

public class TokenRepository
{
    private readonly string _key;
    private readonly ILogger<TokenRepository> _logger;

    public TokenRepository(string key, ILogger<TokenRepository> logger)
    {
        _key = key;
        _logger = logger;
    }
    
    public string GeneratePasswordResetToken(string email)
    {
        try
        {
            _logger.LogInformation("Генерация токена для email: {Email}", email);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            _logger.LogInformation("Токен успешно сгенерирован для email: {Email}", email);
            return jwtToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации токена для email: {Email}", email);
            throw;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        _logger.LogInformation("Начало валидации токена: {Token}", token);

        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Передан пустой или недопустимый токен.");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero // Убираем допущение по времени
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                _logger.LogInformation("Токен успешно валидирован. Subject: {Subject}, Expiration: {Expiration}", 
                    jwtSecurityToken.Subject, jwtSecurityToken.ValidTo);
            }

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Ошибка: токен просрочен.");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Ошибка при валидации токена: недопустимый токен.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неизвестная ошибка при валидации токена.");
            return null;
        }
    }

}
