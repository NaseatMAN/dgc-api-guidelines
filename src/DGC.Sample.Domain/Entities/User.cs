namespace DGC.Sample.Domain.Entities;

public partial class User
{
    public Guid Id { get; init; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
