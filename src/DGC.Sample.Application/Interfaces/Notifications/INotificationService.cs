using DGC.Sample.Application.Dtos.Notifications;

namespace DGC.Sample.Application.Interfaces.Notifications;

public interface INotificationService
{
    Task SendEmailAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default);

    Task SendTelegramAsync(TelegramNotificationMessage message, CancellationToken cancellationToken = default);

    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
