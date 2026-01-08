namespace ApimBilling.Api.Services;

/// <summary>
/// Service to access APIM configuration from the current HTTP request context
/// </summary>
public interface IApimConfigurationProvider
{
    string GetApimServiceName();
    string GetResourceGroup();
    string GetSubscriptionId();
}

/// <summary>
/// Provides APIM configuration from required HTTP headers (X-APIM-ServiceName, X-APIM-ResourceGroup)
/// </summary>
public class ApimConfigurationProvider : IApimConfigurationProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApimConfigurationProvider> _logger;

    public ApimConfigurationProvider(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<ApimConfigurationProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetApimServiceName()
    {
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-APIM-ServiceName"].FirstOrDefault();
        if (string.IsNullOrEmpty(headerValue))
        {
            throw new InvalidOperationException("X-APIM-ServiceName header is required");
        }
        
        _logger.LogDebug("Using APIM service name from header: {ServiceName}", headerValue);
        return headerValue;
    }

    public string GetResourceGroup()
    {
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-APIM-ResourceGroup"].FirstOrDefault();
        if (string.IsNullOrEmpty(headerValue))
        {
            throw new InvalidOperationException("X-APIM-ResourceGroup header is required");
        }
        
        _logger.LogDebug("Using resource group from header: {ResourceGroup}", headerValue);
        return headerValue;
    }

    public string GetSubscriptionId()
    {
        return _configuration["AZURE_SUBSCRIPTION_ID"] 
            ?? throw new InvalidOperationException("AZURE_SUBSCRIPTION_ID is required in configuration");
    }
}
