namespace DGC.Sample.Infrastructure.ExternalServices.Notifications;

public sealed class EmailNotificationSettings
{
    public const string SectionName = "Notifications:Email";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string? FromDisplayName { get; set; }
}
