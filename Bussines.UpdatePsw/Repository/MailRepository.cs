using System.Net;
using System.Net.Mail;
using Inteerfaces;
using Microsoft.Extensions.Configuration;

namespace Bussines.UpdatePsw;

public class MailRepository:IMail
{
    private readonly string SMTPServer;
    private readonly int SMTPPort;
    private readonly string emailPassword;
    private readonly string emailFrom;

    public MailRepository(IConfiguration configuration)
    {
        SMTPPort = 587;
        SMTPServer = configuration["EmailSettings:SmtpServer"];
        emailPassword = configuration["EmailSettings:Password"];
        emailFrom = configuration["EmailSettings:FromEmail"];
    }
    public async Task<bool> SendToken(string email, string token)
    {
        try
        {
            using (SmtpClient client = new SmtpClient(SMTPServer, SMTPPort))
            {
                client.Credentials = new NetworkCredential(emailFrom, emailPassword);
                client.EnableSsl = true;

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Body = token
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}   