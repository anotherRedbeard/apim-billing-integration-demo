namespace ApimBilling.Api.Configuration;

/// <summary>
/// Configuration settings for Azure APIM
/// </summary>
public class ApimSettings
{
    public const string SectionName = "ApimSettings";

    public required string ApimName { get; set; }
    public required string ResourceGroup { get; set; }
    public required string SubscriptionId { get; set; }
}
