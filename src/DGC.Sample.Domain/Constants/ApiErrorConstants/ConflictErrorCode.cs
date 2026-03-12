public static class ConflictErrorCode
{
    public const string DocumentAlreadyExists = "document_already_exists";
    public const string DocumentConflict = "document_conflict";
    public const string IdempotencyKeyProcessing = "idempotency_key_processing";
    public const string IdempotencyKeyReuseMismatch = "idempotency_key_reuse_mismatch";

        public static readonly string[] AllCodes =
        [
            DocumentAlreadyExists,
            DocumentConflict,
            IdempotencyKeyProcessing,
            IdempotencyKeyReuseMismatch
        ];

        public static bool IsValidCode(string code) => AllCodes.Contains(code);
}
