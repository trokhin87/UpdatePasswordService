namespace WebApplication1;

public class SmtpSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
}