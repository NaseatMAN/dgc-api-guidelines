using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Dtos.Notifications;
using DGC.Sample.Application.Services;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Services;

public sealed class NotificationSenderFactoryTests
{
    [Fact]
    public void Get_WhenChannelExists_ShouldReturnMatchingSender()
    {
        var emailSender = Substitute.For<INotificationChannelSender>();
        emailSender.Channel.Returns(NotificationChannel.Email);

        var telegramSender = Substitute.For<INotificationChannelSender>();
        telegramSender.Channel.Returns(NotificationChannel.Telegram);

        var factory = new NotificationSenderFactory(new[] { emailSender, telegramSender });

        var resolved = factory.Get(NotificationChannel.Telegram);

        Assert.Same(telegramSender, resolved);
    }

    [Fact]
    public void Get_WhenChannelMissing_ShouldThrow()
    {
        var emailSender = Substitute.For<INotificationChannelSender>();
        emailSender.Channel.Returns(NotificationChannel.Email);

        var factory = new NotificationSenderFactory(new[] { emailSender });

        Assert.Throws<ArgumentOutOfRangeException>(() => factory.Get(NotificationChannel.Telegram));
    }
}
