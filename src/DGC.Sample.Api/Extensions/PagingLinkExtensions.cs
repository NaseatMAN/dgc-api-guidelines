using Microsoft.AspNetCore.WebUtilities;

namespace DGC.Sample.Api.Extensions;

public static class PagingLinkExtensions
{
    public static string BuildPagingNextLink(this HttpRequest request, int offset, int limit)
    {
        var query = request.Query
            .Where(item => !string.Equals(item.Key, "offset", StringComparison.OrdinalIgnoreCase) &&
                           !string.Equals(item.Key, "limit", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                item => item.Key,
                item => item.Value.ToString());

        query["offset"] = offset.ToString();
        query["limit"] = limit.ToString();

        return QueryHelpers.AddQueryString($"{request.PathBase}{request.Path}", query);
    }
}
