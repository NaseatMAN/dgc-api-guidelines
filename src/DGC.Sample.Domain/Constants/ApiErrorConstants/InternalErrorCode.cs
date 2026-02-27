public static class InternalErrorCode
{
    public const string DatabaseError = "database_error";
    public const string ServiceUnavailable = "service_unavailable";
    public const string ExternalServiceFailure = "external_service_failure";
    public const string UnexpectedError = "unexpected_error";
    public const string QueueTransportInitializationError = "queue_transport_initialization_error";
    public const string QueueTransportNotRegistered = "queue_transport_not_registered";
    public const string QueueProcessingError = "queue_processing_error";

    public static readonly string[] AllCodes =
    [
        DatabaseError,
        ServiceUnavailable,
        ExternalServiceFailure,
        UnexpectedError,
        QueueTransportInitializationError,
        QueueTransportNotRegistered,
        QueueProcessingError
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}