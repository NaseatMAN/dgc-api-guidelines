using System.Text.Json.Serialization;

namespace DGC.Sample.Domain.Exceptions.Errors;

public sealed record AzureErrorResponse(
    [property: JsonPropertyName("error")] AzureError Error);

public sealed record AzureError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("target")] string? Target = null,
    [property: JsonPropertyName("details")] IReadOnlyList<AzureErrorDetail>? Details = null,
    [property: JsonPropertyName("innererror")] AzureInnerError? InnerError = null);

public sealed record AzureErrorDetail(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("target")] string? Target = null);

public sealed record AzureInnerError(
    [property: JsonPropertyName("code")] string? Code = null,
    [property: JsonPropertyName("message")] string? Message = null,
    [property: JsonPropertyName("traceId")] string? TraceId = null,
    [property: JsonPropertyName("innererror")] AzureInnerError? InnerError = null);
