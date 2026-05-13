using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace Jobalatica.Services;

public class BrevoSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}

public class EmailSender : IEmailSender
{
    private readonly BrevoSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<BrevoSettings> settings, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            _logger.LogInformation("Attempting to send email to {Email} with subject {Subject}", email, subject);
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            _logger.LogInformation("Connecting to {Host}:{Port} using {Options}", _settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            
            _logger.LogInformation("Authenticating as {Login}", _settings.Login);
            await client.AuthenticateAsync(_settings.Login, _settings.Password);
            
            _logger.LogInformation("Sending message...");
            await client.SendAsync(message);
            
            _logger.LogInformation("Disconnecting...");
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", email);
            throw; // Re-throw to ensure the calling code knows it failed
        }
    }
}
