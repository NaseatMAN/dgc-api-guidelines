using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EnumDefinedAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var valueType = value.GetType();
        if (!valueType.IsEnum)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be an enum type.");
        }

        if (Enum.IsDefined(valueType, value))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} has an invalid value.");
    }
}
