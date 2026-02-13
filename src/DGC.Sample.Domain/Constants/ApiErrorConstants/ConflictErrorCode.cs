public static class ConflictErrorCode
{
    public const string DocumentAlreadyExists = "document_already_exists";
    public const string DocumentConflict = "document_conflict";

        public static readonly string[] AllCodes =
        [
            DocumentAlreadyExists,
            DocumentConflict
        ];

        public static bool IsValidCode(string code) => AllCodes.Contains(code);
}