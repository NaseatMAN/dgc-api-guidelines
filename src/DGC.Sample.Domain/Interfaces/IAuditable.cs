namespace DGC.Sample.Domain.Interfaces;

public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? ModifiedAtUtc { get; set; }
}
