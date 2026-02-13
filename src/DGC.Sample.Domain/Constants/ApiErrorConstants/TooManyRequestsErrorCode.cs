public static class TooManyRequestsErrorCode
{
    public const string RateLimitExceeded = "rate_limit_exceeded";
    public const string QuotaExceeded = "quota_exceeded";
    public const string RequestThrottled = "request_throttled";

    public static readonly string[] AllCodes =
    [
        RateLimitExceeded,
        QuotaExceeded,
        RequestThrottled
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}