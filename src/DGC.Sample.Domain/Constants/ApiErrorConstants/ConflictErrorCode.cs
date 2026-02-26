public static class ConflictErrorCode
{
    public const string DocumentAlreadyExists = "document_already_exists";
    public const string DocumentConflict = "document_conflict";
    public const string IdempotencyKeyProcessing = "IdempotencyKeyProcessing";

        public static readonly string[] AllCodes =
        [
            DocumentAlreadyExists,
            DocumentConflict,
            IdempotencyKeyProcessing
        ];

        public static bool IsValidCode(string code) => AllCodes.Contains(code);
}