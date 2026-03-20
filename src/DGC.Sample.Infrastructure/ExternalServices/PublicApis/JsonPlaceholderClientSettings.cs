using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Infrastructure.ExternalServices.PublicApis;

public sealed class JsonPlaceholderClientSettings
{
    public const string SectionName = "ExternalApis:JsonPlaceholder";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://jsonplaceholder.typicode.com/";

    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 10;

    [Required]
    public JsonPlaceholderRetrySettings Retry { get; set; } = new();

    [Required]
    public JsonPlaceholderCircuitBreakerSettings CircuitBreaker { get; set; } = new();
}

public sealed class JsonPlaceholderRetrySettings
{
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    [Range(50, 60_000)]
    public int BaseDelayMs { get; set; } = 200;

    [Range(100, 120_000)]
    public int MaxDelayMs { get; set; } = 2_000;
}

public sealed class JsonPlaceholderCircuitBreakerSettings
{
    [Range(0.01, 1.0)]
    public double FailureRatio { get; set; } = 0.2;

    [Range(2, 200)]
    public int MinimumThroughput { get; set; } = 10;

    [Range(1, 300)]
    public int SamplingDurationSeconds { get; set; } = 30;

    [Range(1, 300)]
    public int BreakDurationSeconds { get; set; } = 30;
}
