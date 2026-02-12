public static class NotFoundErrorCode
{
    public const string ResourceNotFound = "resource_not_found";
    public const string EndpointNotFound = "endpoint_not_found";
    public const string ItemNotFound = "item_not_found";

    public static readonly string[] AllCodes =
    [
        ResourceNotFound,
        EndpointNotFound,
        ItemNotFound
    ];

    public static bool IsValidCode(string code) => AllCodes.Contains(code);
}