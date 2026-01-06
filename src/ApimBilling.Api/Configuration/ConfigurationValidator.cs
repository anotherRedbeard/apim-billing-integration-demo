namespace ApimBilling.Api.Configuration;

/// <summary>
/// Validates configuration at startup
/// </summary>
public static class ConfigurationValidator
{
    public static void ValidateApimSettings(ApimSettings settings)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.ApimName))
            errors.Add("APIM_NAME is required");

        if (string.IsNullOrWhiteSpace(settings.ResourceGroup))
            errors.Add("APIM_RESOURCE_GROUP is required");

        if (string.IsNullOrWhiteSpace(settings.SubscriptionId))
            errors.Add("AZURE_SUBSCRIPTION_ID is required");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"Configuration validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
