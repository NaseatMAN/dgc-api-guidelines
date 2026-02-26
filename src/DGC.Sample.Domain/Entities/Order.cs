using System;
using System.Collections.Generic;

namespace DGC.Sample.Domain.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public DateTime OrderDateUtc { get; set; }

    public int Status { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string TenantId { get; set; } = null!;
}
