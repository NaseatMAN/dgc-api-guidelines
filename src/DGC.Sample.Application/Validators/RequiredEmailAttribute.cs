using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class RequiredEmailAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string email && !string.IsNullOrWhiteSpace(email) && EmailAddressValidator.IsValid(email))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid email address.");
    }
}
