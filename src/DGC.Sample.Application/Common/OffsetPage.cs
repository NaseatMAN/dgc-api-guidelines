namespace DGC.Sample.Application.Common;

public sealed class OffsetPage<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Offset { get; init; }
    public int Limit { get; init; }
    public int TotalCount { get; init; }
    public int NextOffset => Offset + Limit;
    public bool HasNextPage => NextOffset < TotalCount;
}
