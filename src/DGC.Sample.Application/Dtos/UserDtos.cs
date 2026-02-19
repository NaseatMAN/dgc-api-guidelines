using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Dtos;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string Email,
    DateTime CreatedAtUtc);

public sealed record UserCreateRequest(
    [Required][StringLength(200)] string FullName,
    [Required][RegularExpression(@"^\d{9,10}$", ErrorMessage = "National ID must be 9 or 10 digits.")] string NationalId,
    [Required][RegularExpression(@"^(0|\+855)[1-9][0-9]{7,8}$", ErrorMessage = "Invalid Cambodia phone number format.")] string PhoneNumber,
    [Required][EmailAddress] string Email);

public sealed record UserUpdateRequest(
    [Required][StringLength(200)] string FullName,
    [Required][RegularExpression(@"^\d{9,10}$", ErrorMessage = "National ID must be 9 or 10 digits.")] string NationalId,
    [Required][RegularExpression(@"^(0|\+855)[1-9][0-9]{7,8}$", ErrorMessage = "Invalid Cambodia phone number format.")] string PhoneNumber,
    [Required][EmailAddress] string Email);

public sealed record UserRegistrationRequest(
    [Required][StringLength(200, MinimumLength = 2)] string FullName,
    [Required][RegularExpression(@"^\d{9,10}$", ErrorMessage = "National ID must be 9 or 10 digits.")] string NationalId,
    [Required][RegularExpression(@"^(0|\+855)[1-9][0-9]{7,8}$", ErrorMessage = "Invalid Cambodia phone number format.")] string PhoneNumber,
    [Required][EmailAddress] string Email,
    [Required][RegularExpression(@"^(User|Admin)$", ErrorMessage = "Role must be either 'User' or 'Admin'.")] string Role,
    [Required][MinLength(8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9]).*$", ErrorMessage = "Password must contain at least one uppercase letter and one number.")]
    string Password,
    [Required] string ConfirmPassword) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // CROSS-FIELD LOGIC: Passwords match
        if (Password != ConfirmPassword)
        {
            yield return new ValidationResult(
                "Passwords do not match.",
                [nameof(ConfirmPassword)]);
        }

        // CONDITIONAL LOGIC: Admin users must register with a corporate email
        if (Role == "Admin" && !Email.EndsWith("@dgc.com", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Admin users must register with a corporate @dgc.com email address.",
                [nameof(Email)]);
        }
    }
}
