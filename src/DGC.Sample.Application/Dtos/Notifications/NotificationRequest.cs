namespace DGC.Sample.Application.Dtos.Notifications;

public sealed record NotificationRequest(
    NotificationChannel Channel,
    string Recipient,
    string Content,
    string? Subject = null,
    bool IsHtml = false,
    string? ParseMode = null);
