using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Dtos.Notifications;
using DGC.Sample.Application.Services;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Services;

public sealed class NotificationServiceTests
{
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly INotificationSenderFactory _notificationSenderFactory;
    private readonly INotificationChannelSender _channelSender;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _telegramSender = Substitute.For<ITelegramSender>();
        _notificationSenderFactory = Substitute.For<INotificationSenderFactory>();
        _channelSender = Substitute.For<INotificationChannelSender>();
        _service = new NotificationService(_emailSender, _telegramSender, _notificationSenderFactory);
    }

    [Fact]
    public async Task SendAsync_ShouldUseSenderResolvedByFactory()
    {
        var request = new NotificationRequest(
            NotificationChannel.Email,
            "user@example.com",
            "Hello",
            "Subject");
        _notificationSenderFactory.Get(NotificationChannel.Email).Returns(_channelSender);

        await _service.SendAsync(request, default);

        _notificationSenderFactory.Received(1).Get(NotificationChannel.Email);
        await _channelSender.Received(1).SendAsync(
            request,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailAsync_ShouldUseEmailSender()
    {
        var message = new EmailNotificationMessage("user@example.com", "Subject", "Hello");

        await _service.SendEmailAsync(message, default);

        await _emailSender.Received(1).SendAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendTelegramAsync_ShouldUseTelegramSender()
    {
        var message = new TelegramNotificationMessage("123456", "Hello from bot", "MarkdownV2");

        await _service.SendTelegramAsync(message, default);

        await _telegramSender.Received(1).SendAsync(message, Arg.Any<CancellationToken>());
    }
}
