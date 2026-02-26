namespace DGC.Sample.Application.Dtos.Notifications;

public sealed record TelegramNotificationMessage(
    string ChatId,
    string Text,
    string? ParseMode = null,
    bool DisableWebPagePreview = true);
