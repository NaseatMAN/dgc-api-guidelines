using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Domain.Specifications.Orders;

public sealed class OrderWithItemsSpec : Specification<Order>
{
    public OrderWithItemsSpec(Guid orderId)
        : base(o => o.Id == orderId)
    {
        // AddInclude(o => o.OrderItems);
        // Note: For deep includes like OrderItems.Product, 
        // the current Specification base might need a more advanced implementation 
        // to support ThenInclude. 
        // For this simple example, we'll demonstrate a base include.
    }
}
