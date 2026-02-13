public static class BadRequestErrorCode
{
    public const string InvalidModelError = "invalid_document";
    public const string DocumentExpired = "expired_document";

    public static readonly string[] AllCodes =
    [
        InvalidModelError,
        DocumentExpired
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}