using DGC.Sample.Application.Dtos.Notifications;

namespace DGC.Sample.Application.Interfaces.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailNotificationMessage message, CancellationToken cancellationToken);
}
