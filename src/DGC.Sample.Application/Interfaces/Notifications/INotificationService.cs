using DGC.Sample.Application.Dtos.Notifications;

namespace DGC.Sample.Application.Interfaces.Notifications;

public interface INotificationService
{
    Task SendEmailAsync(EmailNotificationMessage message, CancellationToken cancellationToken);

    Task SendTelegramAsync(TelegramNotificationMessage message, CancellationToken cancellationToken);

    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken);
}
