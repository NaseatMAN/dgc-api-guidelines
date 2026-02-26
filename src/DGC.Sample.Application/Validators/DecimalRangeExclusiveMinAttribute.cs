using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class DecimalRangeExclusiveMinAttribute : ValidationAttribute
{
    private readonly decimal _minimumExclusive;
    private readonly decimal _maximumInclusive;

    public DecimalRangeExclusiveMinAttribute(double minimumExclusive, double maximumInclusive)
    {
        _minimumExclusive = Convert.ToDecimal(minimumExclusive);
        _maximumInclusive = Convert.ToDecimal(maximumInclusive);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not decimal amount)
        {
            return ValidationResult.Success;
        }

        if (amount > _minimumExclusive && amount <= _maximumInclusive)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            ErrorMessage ??
            $"{validationContext.DisplayName} must be greater than {_minimumExclusive} and less than or equal to {_maximumInclusive}.");
    }
}
