using DGC.Sample.Application.Dtos.Notifications;

namespace DGC.Sample.Application.Interfaces.Notifications;

public interface INotificationChannelSender
{
    NotificationChannel Channel { get; }

    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
