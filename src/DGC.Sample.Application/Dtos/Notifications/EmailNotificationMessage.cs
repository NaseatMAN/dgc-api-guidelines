namespace DGC.Sample.Application.Dtos.Notifications;

public sealed record EmailNotificationMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = false);
