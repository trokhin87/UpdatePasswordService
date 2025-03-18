using System.Net;
using System.Net.Mail;
using Inteerfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bussines.UpdatePsw;

public class MailRepository : IMail
{
    private readonly string SMTPServer;
    private readonly int SMTPPort;
    private readonly string emailPassword;
    private readonly string emailFrom;
    private readonly ILogger<MailRepository> _logger;

    public MailRepository(IConfiguration configuration, ILogger<MailRepository> logger)
    {
        SMTPPort = 587;
        SMTPServer = configuration["EmailSettings:SmtpServer"];
        emailPassword = configuration["EmailSettings:Password"];
        emailFrom = configuration["EmailSettings:FromEmail"];
        _logger = logger;
    }

    public async Task<bool> SendToken(string email, string token)
    {
        try
        {
            _logger.LogInformation("Отправка токена на email: {Email}", email);

            using (SmtpClient client = new SmtpClient(SMTPServer, SMTPPort))
            {
                client.Credentials = new NetworkCredential(emailFrom, emailPassword);
                client.EnableSsl = true;

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Subject = "Сброс пароля",
                    Body = $"Ваш токен для сброса пароля: {token}",
                    IsBodyHtml = false
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation("Токен успешно отправлен на email: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке токена на email: {Email}", email);
            return false;
        }
    }
}