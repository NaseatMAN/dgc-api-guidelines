using DGC.Sample.Application.Dtos.Notifications;

namespace DGC.Sample.Application.Interfaces.Notifications;

public interface ITelegramSender
{
    Task SendAsync(TelegramNotificationMessage message, CancellationToken cancellationToken = default);
}
