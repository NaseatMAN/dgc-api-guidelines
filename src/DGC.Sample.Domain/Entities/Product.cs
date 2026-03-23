using System;
using System.Collections.Generic;

namespace DGC.Sample.Domain.Entities;

public partial class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int AvailableStock { get; set; }
}
