using DGC.Sample.Domain.Interfaces;

namespace DGC.Sample.Domain.Entities;

public partial class User : ISoftDeletable, ITenantEntity, IAuditable
{
}
