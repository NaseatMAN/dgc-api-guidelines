using System.Linq.Expressions;
using DGC.Sample.Domain.Entities;


namespace DGC.Sample.Domain.Specifications.Orders;

public sealed class OrderIncludingDeletedSpec : Specification<Order>
{
    public OrderIncludingDeletedSpec(Expression<Func<Order, bool>>? criteria = null) 
        : base(criteria)
    {
        IgnoreNamedFilter("SoftDeleteFilter");
    }
}
