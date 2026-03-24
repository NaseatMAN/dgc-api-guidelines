using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Dtos;

public sealed class OffsetPagingRequest
{
    [Range(0, int.MaxValue)]
    public int Offset { get; init; } = 0;

    [Range(1, 100)]
    public int Limit { get; init; } = 50;
}
