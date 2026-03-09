namespace DGC.Sample.Domain.Constants.ApiErrorConstants;

public static class BadRequestErrorCode
{
    public const string InvalidModelError = "invalid_document";
    public const string DocumentExpired = "expired_document";
    public const string MissingApiVersionParameter = "missing_api_version_parameter";
    public const string UnsupportedApiVersionValue = "unsupported_api_version_value";

    public static readonly string[] AllCodes =
    [
        InvalidModelError,
        DocumentExpired,
        MissingApiVersionParameter,
        UnsupportedApiVersionValue
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}