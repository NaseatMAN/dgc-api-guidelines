public static class InternalErrorCode
{
    public const string DatabaseError = "database_error";
    public const string ServiceUnavailable = "service_unavailable";
    public const string ExternalServiceFailure = "external_service_failure";
    public const string UnexpectedError = "unexpected_error";

    public static readonly string[] AllCodes =
    [
        DatabaseError,
        ServiceUnavailable,
        ExternalServiceFailure,
        UnexpectedError
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}