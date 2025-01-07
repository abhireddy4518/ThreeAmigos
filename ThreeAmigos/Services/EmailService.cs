using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace ThreeAmigos.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:SenderEmail"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_configuration["EmailSettings:Host"],
                                int.Parse(_configuration["EmailSettings:Port"]),
                                MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_configuration["EmailSettings:Username"],
                                     _configuration["EmailSettings:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
