using System.Net;
using System.Net.Mail;
using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Dtos.Notifications;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Infrastructure.ExternalServices.Notifications;

public sealed class SmtpEmailSender(
    EmailNotificationSettings settings,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailNotificationSettings _settings = settings;
    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public async Task SendAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email notification skipped because Notifications:Email:Enabled is false.");
            return;
        }

        ValidateConfiguration();

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var mailMessage = new MailMessage
        {
            From = string.IsNullOrWhiteSpace(_settings.FromDisplayName)
                ? new MailAddress(_settings.FromAddress)
                : new MailAddress(_settings.FromAddress, _settings.FromDisplayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };
        mailMessage.To.Add(message.To);

        await smtpClient.SendMailAsync(mailMessage).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new InvalidOperationException("Notifications:Email:Host is required.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            throw new InvalidOperationException("Notifications:Email:FromAddress is required.");
        }
    }
}
