using System.Text;
using System.Text.Json;
using ApimBilling.Contracts;
using ApimBilling.Web.Models;

namespace ApimBilling.Web.Services;

/// <summary>
/// HTTP client for communicating with the Billing API backend
/// </summary>
public interface IBillingApiClient
{
    Task<List<Product>> GetProductsAsync();
    Task<PurchaseResponse> PurchaseProductAsync(PurchaseRequest request);
    Task<SubscriptionInfo> GetSubscriptionAsync(string subscriptionId);
    Task<List<SubscriptionInfo>> GetSubscriptionsByEmailAsync(string email);
    Task<SubscriptionInfo> UpdateSubscriptionStateAsync(string subscriptionId, string action);
    Task RotateKeyAsync(string subscriptionId, string keyType);
    Task DeleteSubscriptionAsync(string subscriptionId);
}

public class BillingApiClient : IBillingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BillingApiClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public BillingApiClient(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<BillingApiClient> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        
        // Set base URL from configuration
        var baseUrl = _configuration["BillingApi:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    /// <summary>
    /// Adds APIM headers to the request if available in session
    /// </summary>
    private void AddApimHeaders(HttpRequestMessage request)
    {
        var serviceName = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.ApimServiceName);
        var resourceGroup = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.ApimResourceGroup);

        if (!string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(resourceGroup))
        {
            request.Headers.Add("X-APIM-ServiceName", serviceName);
            request.Headers.Add("X-APIM-ResourceGroup", resourceGroup);
            _logger.LogDebug("Added APIM headers: ServiceName={ServiceName}, ResourceGroup={ResourceGroup}", 
                serviceName, resourceGroup);
        }
        else
        {
            _logger.LogWarning("APIM configuration not found in session. Backend will use default configuration.");
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        _logger.LogInformation("Fetching products from Billing API");
        
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var products = await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
        
        _logger.LogInformation("Retrieved {Count} products", products.Count);
        
        return products;
    }

    public async Task<PurchaseResponse> PurchaseProductAsync(PurchaseRequest purchaseRequest)
    {
        _logger.LogInformation("Purchasing product: {ProductId} for {Email}", 
            purchaseRequest.ProductId, purchaseRequest.CustomerEmail);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/subscriptions/purchase")
        {
            Content = JsonContent.Create(purchaseRequest)
        };
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PurchaseResponse>() 
            ?? throw new InvalidOperationException("Failed to deserialize purchase response");
        
        _logger.LogInformation("Purchase successful - Subscription: {SubscriptionId}", result.SubscriptionId);
        
        return result;
    }

    public async Task<SubscriptionInfo> GetSubscriptionAsync(string subscriptionId)
    {
        _logger.LogInformation("Getting subscription: {SubscriptionId}", subscriptionId);
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/subscriptions/{subscriptionId}");
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<SubscriptionInfo>() 
            ?? throw new InvalidOperationException("Failed to deserialize subscription info");
    }

    public async Task<List<SubscriptionInfo>> GetSubscriptionsByEmailAsync(string email)
    {
        _logger.LogInformation("Getting subscriptions for email: {Email}", email);
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/subscriptions?email={Uri.EscapeDataString(email)}");
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<SubscriptionInfo>>() ?? new List<SubscriptionInfo>();
    }

    public async Task<SubscriptionInfo> UpdateSubscriptionStateAsync(string subscriptionId, string action)
    {
        _logger.LogInformation("Updating subscription {SubscriptionId} - Action: {Action}", 
            subscriptionId, action);
        
        var updateRequest = new UpdateSubscriptionRequest { Action = action };
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/subscriptions/{subscriptionId}/state")
        {
            Content = JsonContent.Create(updateRequest)
        };
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<SubscriptionInfo>() 
            ?? throw new InvalidOperationException("Failed to deserialize subscription info");
    }

    public async Task RotateKeyAsync(string subscriptionId, string keyType)
    {
        _logger.LogInformation("Rotating {KeyType} key for subscription {SubscriptionId}", 
            keyType, subscriptionId);
        
        var rotateRequest = new RotateKeyRequest { KeyType = keyType };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/subscriptions/{subscriptionId}/rotate-key")
        {
            Content = JsonContent.Create(rotateRequest)
        };
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSubscriptionAsync(string subscriptionId)
    {
        _logger.LogInformation("Deleting subscription: {SubscriptionId}", subscriptionId);
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/subscriptions/{subscriptionId}");
        AddApimHeaders(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
