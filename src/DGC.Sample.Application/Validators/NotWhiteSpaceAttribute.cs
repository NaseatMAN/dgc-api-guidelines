using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class NotWhiteSpaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
    }
}
