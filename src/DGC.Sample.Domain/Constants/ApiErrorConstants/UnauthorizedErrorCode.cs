public static class UnauthorizedErrorCode
{
    public const string InvalidCredentials = "invalid_credentials";
    public const string TokenExpired = "token_expired";
    public const string AuthenticationRequired = "authentication_required";

    public static readonly string[] AllCodes =
    [
        InvalidCredentials,
        TokenExpired,
        AuthenticationRequired
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}