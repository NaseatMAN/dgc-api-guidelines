using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class CambodiaPhoneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || !PhoneRegex().IsMatch(text))
        {
            return new ValidationResult(
                ErrorMessage ?? "Phone number must be a valid Cambodia phone number (e.g., 012345678 or +85512345678).");
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex(@"^(0|\+855)[1-9][0-9]{7,8}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();
}
