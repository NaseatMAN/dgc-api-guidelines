using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Domain.Entities;

public partial class Order
{
    public Guid Id { get; init; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}
