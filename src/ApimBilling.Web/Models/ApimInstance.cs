namespace ApimBilling.Web.Models;

/// <summary>
/// Represents an APIM instance configuration
/// </summary>
public class ApimInstance
{
    /// <summary>
    /// Azure APIM service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Azure resource group containing the APIM instance
    /// </summary>
    public string ResourceGroup { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name shown to users in the UI (defaults to ServiceName if not provided)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional description of the APIM instance
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Session keys for storing APIM configuration
/// </summary>
public static class SessionKeys
{
    public const string UserEmail = "UserEmail";
    public const string UserName = "UserName";
    public const string ApimServiceName = "ApimServiceName";
    public const string ApimResourceGroup = "ApimResourceGroup";
}
