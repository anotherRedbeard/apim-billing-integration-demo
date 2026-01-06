using System.Text;
using System.Text.Json;
using ApimBilling.Contracts;

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

    public BillingApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<BillingApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var apiUrl = configuration["BillingApi:BaseUrl"] 
            ?? throw new InvalidOperationException("BillingApi:BaseUrl configuration is required");
        
        _httpClient.BaseAddress = new Uri(apiUrl);
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        _logger.LogInformation("Fetching products from Billing API");
        
        var response = await _httpClient.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        
        var products = await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
        
        _logger.LogInformation("Retrieved {Count} products", products.Count);
        
        return products;
    }

    public async Task<PurchaseResponse> PurchaseProductAsync(PurchaseRequest request)
    {
        _logger.LogInformation("Purchasing product: {ProductId} for {Email}", 
            request.ProductId, request.CustomerEmail);
        
        var response = await _httpClient.PostAsJsonAsync("/api/subscriptions/purchase", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PurchaseResponse>() 
            ?? throw new InvalidOperationException("Failed to deserialize purchase response");
        
        _logger.LogInformation("Purchase successful - Subscription: {SubscriptionId}", result.SubscriptionId);
        
        return result;
    }

    public async Task<SubscriptionInfo> GetSubscriptionAsync(string subscriptionId)
    {
        _logger.LogInformation("Getting subscription: {SubscriptionId}", subscriptionId);
        
        var response = await _httpClient.GetAsync($"/api/subscriptions/{subscriptionId}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<SubscriptionInfo>() 
            ?? throw new InvalidOperationException("Failed to deserialize subscription info");
    }

    public async Task<List<SubscriptionInfo>> GetSubscriptionsByEmailAsync(string email)
    {
        _logger.LogInformation("Getting subscriptions for email: {Email}", email);
        
        var response = await _httpClient.GetAsync($"/api/subscriptions?email={Uri.EscapeDataString(email)}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<SubscriptionInfo>>() ?? new List<SubscriptionInfo>();
    }

    public async Task<SubscriptionInfo> UpdateSubscriptionStateAsync(string subscriptionId, string action)
    {
        _logger.LogInformation("Updating subscription {SubscriptionId} - Action: {Action}", 
            subscriptionId, action);
        
        var request = new UpdateSubscriptionRequest { Action = action };
        var response = await _httpClient.PatchAsJsonAsync($"/api/subscriptions/{subscriptionId}/state", request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<SubscriptionInfo>() 
            ?? throw new InvalidOperationException("Failed to deserialize subscription info");
    }

    public async Task RotateKeyAsync(string subscriptionId, string keyType)
    {
        _logger.LogInformation("Rotating {KeyType} key for subscription {SubscriptionId}", 
            keyType, subscriptionId);
        
        var request = new RotateKeyRequest { KeyType = keyType };
        var response = await _httpClient.PostAsJsonAsync($"/api/subscriptions/{subscriptionId}/rotate-key", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSubscriptionAsync(string subscriptionId)
    {
        _logger.LogInformation("Deleting subscription: {SubscriptionId}", subscriptionId);
        
        var response = await _httpClient.DeleteAsync($"/api/subscriptions/{subscriptionId}");
        response.EnsureSuccessStatusCode();
    }
}
