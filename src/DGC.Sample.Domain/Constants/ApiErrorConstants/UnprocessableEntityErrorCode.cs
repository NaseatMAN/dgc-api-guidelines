public static class UnprocessableEntityErrorCode
{
    public const string ValidationError = "validation_error";
    public const string InvalidFormat = "invalid_format";
    public const string BusinessRuleViolation = "business_rule_violation";

    public static readonly string[] AllCodes =
    [
        ValidationError,
        InvalidFormat,
        BusinessRuleViolation
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}