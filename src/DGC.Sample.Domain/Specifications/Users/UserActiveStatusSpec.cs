

using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Domain.Specifications.Users;

public sealed class UserActiveStatusSpec : Specification<User>
{
    public UserActiveStatusSpec(bool isActive = true)
        : base(u => u.IsActive == isActive)
    {
    }
}
