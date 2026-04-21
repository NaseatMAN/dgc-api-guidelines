using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using PhoneNumbers;

namespace DGC.Sample.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class CambodiaPhoneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (value is not string str)
            {
                throw new Exception("field value is not string");
            }

            var phoneNumberUtil = validationContext.GetRequiredService<PhoneNumberUtil>();
            var phoneNumberObject = phoneNumberUtil.Parse(str, "KH");
            if (!phoneNumberUtil.IsValidNumber(phoneNumberObject))
            {
                throw new Exception("phone number is not valid by google libphonenumber format");
            }

            return ValidationResult.Success;
        }
        catch
        {
            return new ValidationResult(
                "The value is not a Cambodian Phone Number type. eg: 017123456, 85512333444, +8551234445566",
                [validationContext.MemberName ?? string.Empty]);
        }
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
    }
}
