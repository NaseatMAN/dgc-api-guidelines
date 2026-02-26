using System;
using System.Collections.Generic;

namespace DGC.Sample.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string NationalId { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string TenantId { get; set; } = null!;

    public bool IsActive { get; set; }
}
