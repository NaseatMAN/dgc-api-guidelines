using FluentValidation;
using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Validators;

public sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .Matches(@"^\d{9,10}$")
            .WithMessage("National ID must be a valid 9 or 10-digit Cambodia National ID.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(0|\+855)[1-9][0-9]{7,8}$")
            .WithMessage("Phone number must be a valid Cambodia phone number (e.g., 012345678 or +85512345678).");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
