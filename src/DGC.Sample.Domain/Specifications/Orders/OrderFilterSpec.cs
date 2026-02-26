using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Enums;

namespace DGC.Sample.Domain.Specifications.Orders;

public sealed class OrderFilterSpec : Specification<Order>
{
    public OrderFilterSpec(OrderStatus? status, string? customerNameSearch)
        : base(o => (!status.HasValue || o.Status == status.Value) &&
                    (string.IsNullOrWhiteSpace(customerNameSearch) || o.CustomerName.Contains(customerNameSearch)))
    {
        ApplyOrderByDescending(o => o.OrderDateUtc);
    }
}
