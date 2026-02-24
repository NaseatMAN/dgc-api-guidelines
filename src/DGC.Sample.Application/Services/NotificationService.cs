using DGC.Sample.Application.Dtos.Notifications;
using DGC.Sample.Application.Interfaces.Notifications;

namespace DGC.Sample.Application.Services;

public sealed class NotificationService(
    IEmailSender emailSender,
    ITelegramSender telegramSender,
    INotificationSenderFactory notificationSenderFactory) : INotificationService
{
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ITelegramSender _telegramSender = telegramSender;
    private readonly INotificationSenderFactory _notificationSenderFactory = notificationSenderFactory;

    public Task SendEmailAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default)
    {
        return _emailSender.SendAsync(message, cancellationToken);
    }

    public Task SendTelegramAsync(TelegramNotificationMessage message, CancellationToken cancellationToken = default)
    {
        return _telegramSender.SendAsync(message, cancellationToken);
    }

    public Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var sender = _notificationSenderFactory.Get(request.Channel);
        return sender.SendAsync(request, cancellationToken);
    }
}
