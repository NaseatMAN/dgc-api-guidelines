using DGC.Sample.Domain.Interfaces;

namespace DGC.Sample.Domain.Entities;

public partial class Order : ISoftDeletable, ITenantEntity, IAuditable
{
}
