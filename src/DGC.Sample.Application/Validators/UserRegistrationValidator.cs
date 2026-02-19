using FluentValidation;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;

namespace DGC.Sample.Application.Validators;

public sealed class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly IUserRepository _userRepository;

    public UserRegistrationValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.FullName)
            .NotEmpty()
            .Length(2, 200);

        // ASYNC CHECK: Impossible with standard attributes
        RuleFor(x => x.NationalId)
            .NotEmpty()
            .Matches(@"^\d{9,10}$").WithMessage("National ID must be 9 or 10 digits.")
            .MustAsync(BeUniqueNationalId).WithMessage("National ID already exists in our records.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(0|\+855)[1-9][0-9]{7,8}$").WithMessage("Invalid Cambodia phone number format.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        // CONDITIONAL LOGIC: Impossible with simple attributes
        RuleFor(x => x.Email)
            .Must(email => email.EndsWith("@dgc.com", StringComparison.OrdinalIgnoreCase))
            .When(x => x.Role == "Admin")
            .WithMessage("Admin users must register with a corporate @dgc.com email address.");

        RuleFor(x => x.Role)
            .Must(role => role is "User" or "Admin").WithMessage("Role must be either 'User' or 'Admin'.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");

        // CROSS-FIELD LOGIC: Type-safe and readable
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }

    private async Task<bool> BeUniqueNationalId(string nationalId, CancellationToken token)
    {
        return !await _userRepository.ExistsByNationalIdAsync(nationalId, token);
    }
}
