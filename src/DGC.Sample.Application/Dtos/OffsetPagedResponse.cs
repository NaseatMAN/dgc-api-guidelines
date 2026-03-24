namespace DGC.Sample.Application.Dtos;

public sealed class OffsetPagedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Offset { get; init; }
    public int Limit { get; init; }
    public int TotalCount { get; init; }
    public string? NextLink { get; init; }
}
