using System.Net;
using System.Net.Http.Json;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Infrastructure.ExternalServices.PublicApis;

public sealed class JsonPlaceholderUserClient(
    IHttpClientFactory httpClientFactory,
    ILogger<JsonPlaceholderUserClient> logger) : IPublicUserLookupClient
{
    public const string ClientName = "JsonPlaceholder";

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<JsonPlaceholderUserClient> _logger = logger;

    public async Task<PublicUserResponse?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(ClientName);

        using var response = await client.GetAsync($"users/{id}", cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonPlaceholderUserPayload>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (payload is null)
        {
            _logger.LogWarning("Public API returned an empty payload for user id {UserId}.", id);
            return null;
        }

        return new PublicUserResponse(
            payload.Id,
            payload.Name,
            payload.Username,
            payload.Email,
            payload.Phone,
            payload.Website);
    }

    private sealed record JsonPlaceholderUserPayload(
        int Id,
        string Name,
        string Username,
        string Email,
        string? Phone,
        string? Website);
}
