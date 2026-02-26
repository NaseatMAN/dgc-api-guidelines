using DGC.Sample.Application.Dtos.Notifications;
using DGC.Sample.Application.Interfaces.Notifications;

namespace DGC.Sample.Application.Services;

public sealed class NotificationSenderFactory(IEnumerable<INotificationChannelSender> senders) : INotificationSenderFactory
{
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationChannelSender> _senderMap =
        senders.GroupBy(sender => sender.Channel)
            .ToDictionary(group => group.Key, group => group.Last());

    public INotificationChannelSender Get(NotificationChannel channel)
    {
        if (_senderMap.TryGetValue(channel, out var sender))
        {
            return sender;
        }

        throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unsupported notification channel.");
    }
}
