namespace DGC.Sample.Functions.Configuration;

public sealed class AzureFunctionsQueueOptions
{
    public const string SectionName = "AzureFunctions";

    public string QueueName { get; set; } = string.Empty;
}