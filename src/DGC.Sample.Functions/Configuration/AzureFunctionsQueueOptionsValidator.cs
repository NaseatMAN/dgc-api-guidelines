using Microsoft.Extensions.Options;

namespace DGC.Sample.Functions.Configuration;

public sealed class AzureFunctionsQueueOptionsValidator : IValidateOptions<AzureFunctionsQueueOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureFunctionsQueueOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.QueueName))
        {
            return ValidateOptionsResult.Fail("Configuration key 'AzureFunctions:QueueName' is required.");
        }

        var queueName = options.QueueName.Trim();

        if (queueName.Length is < 3 or > 63)
        {
            return ValidateOptionsResult.Fail("'AzureFunctions:QueueName' must be between 3 and 63 characters.");
        }

        if (!char.IsLetterOrDigit(queueName[0]) || !char.IsLetterOrDigit(queueName[^1]))
        {
            return ValidateOptionsResult.Fail("'AzureFunctions:QueueName' must start and end with a lowercase letter or number.");
        }

        if (queueName.Any(c => !(char.IsLower(c) || char.IsDigit(c) || c == '-')))
        {
            return ValidateOptionsResult.Fail("'AzureFunctions:QueueName' can contain only lowercase letters, numbers, and '-'.");
        }

        if (queueName.Contains("--", StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Fail("'AzureFunctions:QueueName' cannot contain consecutive hyphens.");
        }

        return ValidateOptionsResult.Success;
    }
}