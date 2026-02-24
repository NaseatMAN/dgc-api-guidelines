using DGC.Sample.Application.Dtos.Notifications;
using DGC.Sample.Application.Interfaces.Notifications;

namespace DGC.Sample.Application.Services;

public sealed class TelegramNotificationChannelSender(ITelegramSender telegramSender) : INotificationChannelSender
{
    private readonly ITelegramSender _telegramSender = telegramSender;

    public NotificationChannel Channel => NotificationChannel.Telegram;

    public Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var message = new TelegramNotificationMessage(
            request.Recipient,
            request.Content,
            request.ParseMode);

        return _telegramSender.SendAsync(message, cancellationToken);
    }
}
