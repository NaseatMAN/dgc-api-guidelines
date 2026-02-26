using DGC.Sample.Application.Dtos.Notifications;

namespace DGC.Sample.Application.Interfaces.Notifications;

public interface INotificationSenderFactory
{
    INotificationChannelSender Get(NotificationChannel channel);
}
