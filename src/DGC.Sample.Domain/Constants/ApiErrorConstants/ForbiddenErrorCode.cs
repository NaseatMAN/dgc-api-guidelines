public static class ForbiddenErrorCode
{
    public const string AccessDenied = "access_denied";
    public const string InsufficientPermissions = "insufficient_permissions";
    public const string ResourceAccessForbidden = "resource_access_forbidden";

    public static readonly string[] AllCodes =
    [
        AccessDenied,
        InsufficientPermissions,
        ResourceAccessForbidden
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}