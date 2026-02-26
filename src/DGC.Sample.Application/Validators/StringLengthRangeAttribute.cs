using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class StringLengthRangeAttribute : ValidationAttribute
{
    private readonly int _min;
    private readonly int _max;

    public StringLengthRangeAttribute(int minimumLength, int maximumLength)
    {
        _min = minimumLength;
        _max = maximumLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text)
        {
            return ValidationResult.Success;
        }

        if (text.Length < _min || text.Length > _max)
        {
            return new ValidationResult(
                ErrorMessage ?? $"{validationContext.DisplayName} must be between {_min} and {_max} characters.");
        }

        return ValidationResult.Success;
    }
}
