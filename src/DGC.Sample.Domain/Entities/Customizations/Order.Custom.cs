using DGC.Sample.Domain.Interfaces;

namespace DGC.Sample.Domain.Entities;

public partial class Order : ISoftDeletable, ITenantEntity, IAuditable
{
    public bool IsDeleted { get; set; } = false;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
}
