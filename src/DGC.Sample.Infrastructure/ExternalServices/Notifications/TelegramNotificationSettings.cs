namespace DGC.Sample.Infrastructure.ExternalServices.Notifications;

public sealed class TelegramNotificationSettings
{
    public const string SectionName = "Notifications:Telegram";

    public bool Enabled { get; set; }

    public string BotToken { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.telegram.org/";
}
