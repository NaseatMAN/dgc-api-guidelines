using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NonDefaultDateTimeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime && dateTime != default)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
    }
}
