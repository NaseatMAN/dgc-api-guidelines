using System.Linq.Expressions;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Domain.Specifications.Orders;

public sealed class OrderIncludingDeletedSpec : Specification<Order>
{
    public OrderIncludingDeletedSpec(Expression<Func<Order, bool>>? criteria = null) 
        : base(criteria)
    {
        // Use the new EF Core 10 Named Filter feature!
        IgnoreNamedFilter("SoftDeleteFilter");
        
        // Ensure we only see current tenant even if ignoring soft delete
        // Note: The TenantFilter remains active!
    }
}
