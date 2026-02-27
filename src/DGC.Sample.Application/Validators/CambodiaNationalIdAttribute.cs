using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class CambodiaNationalIdAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || !NationalIdRegex().IsMatch(text))
        {
            return new ValidationResult(
                ErrorMessage ?? "National ID must be a valid 9 or 10-digit Cambodia National ID.");
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex(@"^\d{9,10}$", RegexOptions.CultureInvariant)]
    private static partial Regex NationalIdRegex();
}
