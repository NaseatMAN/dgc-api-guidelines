using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Domain.Specifications.Orders;

public sealed class OrderActiveStatusSpec : Specification<Order>
{
    public OrderActiveStatusSpec(bool isActive = true)
        : base(o => o.IsActive == isActive)
    {
    }
}
