using DGC.Sample.Application.Dtos.Notifications;
using DGC.Sample.Application.Interfaces.Notifications;

namespace DGC.Sample.Application.Services;

public sealed class EmailNotificationChannelSender(IEmailSender emailSender) : INotificationChannelSender
{
    private readonly IEmailSender _emailSender = emailSender;

    public NotificationChannel Channel => NotificationChannel.Email;

    public Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var message = new EmailNotificationMessage(
            request.Recipient,
            request.Subject ?? "(no subject)",
            request.Content,
            request.IsHtml);

        return _emailSender.SendAsync(message, cancellationToken);
    }
}
