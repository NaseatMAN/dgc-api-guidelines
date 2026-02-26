using System.Net.Http.Json;
using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Dtos.Notifications;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Infrastructure.ExternalServices.Notifications;

public sealed class TelegramSender(
    TelegramNotificationSettings settings,
    ILogger<TelegramSender> logger) : ITelegramSender
{
    private static readonly HttpClient HttpClient = new();
    private readonly TelegramNotificationSettings _settings = settings;
    private readonly ILogger<TelegramSender> _logger = logger;

    public async Task SendAsync(TelegramNotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Telegram notification skipped because Notifications:Telegram:Enabled is false.");
            return;
        }

        ValidateConfiguration();

        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var endpoint = $"{baseUrl}/bot{_settings.BotToken}/sendMessage";

        var payload = new Dictionary<string, object?>
        {
            ["chat_id"] = message.ChatId,
            ["text"] = message.Text,
            ["parse_mode"] = message.ParseMode,
            ["disable_web_page_preview"] = message.DisableWebPagePreview
        };

        using var response = await HttpClient.PostAsJsonAsync(endpoint, payload, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogError(
            "Telegram send failed. statusCode={StatusCode} body={ResponseBody}",
            (int)response.StatusCode,
            responseBody);

        throw new InvalidOperationException($"Telegram send failed with status code {(int)response.StatusCode}.");
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.BotToken))
        {
            throw new InvalidOperationException("Notifications:Telegram:BotToken is required.");
        }
    }
}
