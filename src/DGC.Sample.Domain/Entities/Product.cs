namespace DGC.Sample.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int AvailableStock { get; set; }
}
