using ApimBilling.Api.Configuration;
using ApimBilling.Api.Models;
using ApimBilling.Contracts;

namespace ApimBilling.Api.Services;

/// <summary>
/// Service that orchestrates billing operations and APIM subscription management
/// </summary>
public interface IBillingService
{
    Task<List<Product>> GetProductsAsync();
    Task<PurchaseResponse> ProcessPurchaseAsync(PurchaseRequest request);
    Task<SubscriptionInfo> GetSubscriptionInfoAsync(string subscriptionId);
    Task<List<SubscriptionInfo>> GetSubscriptionsByEmailAsync(string? email);
    Task<SubscriptionInfo> UpdateSubscriptionAsync(string subscriptionId, string action);
    Task RotateKeyAsync(string subscriptionId, string keyType);
    Task CancelSubscriptionAsync(string subscriptionId);
}

public class BillingService : IBillingService
{
    private readonly IApimSubscriptionClient _apimClient;
    private readonly ApimSettings _settings;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        IApimSubscriptionClient apimClient,
        ApimSettings settings,
        ILogger<BillingService> logger)
    {
        _apimClient = apimClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        _logger.LogInformation("Fetching products from APIM");
        
        var apimProducts = await _apimClient.ListApimProductsAsync();
        
        var products = apimProducts.Value?.
            Where(p => p.Properties?.State == "published")
            .Select(p => new Product
            {
                ProductId = p.Name ?? "",
                Name = p.Properties?.DisplayName ?? p.Name ?? "",
                Description = p.Properties?.Description ?? "",
                State = p.Properties?.State,
                SubscriptionRequired = p.Properties?.SubscriptionRequired ?? true
            })
            .ToList() ?? new List<Product>();

        _logger.LogInformation("Retrieved {Count} published products from APIM", products.Count);
        
        return products;
    }

    public async Task<PurchaseResponse> ProcessPurchaseAsync(PurchaseRequest request)
    {
        _logger.LogInformation("Processing purchase for {Email} - ProductId: {ProductId}", 
            request.CustomerEmail, request.ProductId);

        // Validate product exists in APIM
        var products = await GetProductsAsync();
        var product = products.FirstOrDefault(p => p.ProductId == request.ProductId)
            ?? throw new InvalidOperationException($"Product not found: {request.ProductId}");

        // Create or get APIM user
        var nameParts = request.CustomerName.Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";
        
        var user = await _apimClient.CreateOrGetUserAsync(request.CustomerEmail, firstName, lastName);
        
        _logger.LogInformation("User created/retrieved: {UserId}", user.Id);

        // Create unique subscription name
        var subscriptionName = GenerateSubscriptionName(request.CustomerEmail, request.ProductId);
        var displayName = $"{request.CustomerName} - {product.Name}";

        // Create APIM subscription with user as owner
        var subscription = await _apimClient.CreateSubscriptionAsync(
            subscriptionName,
            request.ProductId,
            displayName,
            user.Id);

        // Get the keys
        var keys = await _apimClient.GetSubscriptionKeysAsync(subscriptionName);

        _logger.LogInformation("Purchase completed - Subscription: {SubscriptionName}", subscriptionName);

        return new PurchaseResponse
        {
            SubscriptionId = subscription.Name!,
            SubscriptionName = subscription.Properties!.DisplayName!,
            PrimaryKey = keys.PrimaryKey!,
            SecondaryKey = keys.SecondaryKey!,
            ProductId = request.ProductId,
            ProductName = product.Name,
            State = subscription.Properties.State!,
            CreatedDate = subscription.Properties.CreatedDate
        };
    }

    public async Task<SubscriptionInfo> GetSubscriptionInfoAsync(string subscriptionId)
    {
        var subscription = await _apimClient.GetSubscriptionAsync(subscriptionId);
        var keys = await _apimClient.GetSubscriptionKeysAsync(subscriptionId);

        var productId = ExtractProductIdFromScope(subscription.Properties?.Scope);

        return new SubscriptionInfo
        {
            SubscriptionId = subscription.Name!,
            SubscriptionName = subscription.Properties!.DisplayName!,
            State = subscription.Properties.State!,
            ProductId = productId,
            ProductName = productId,
            PrimaryKey = keys.PrimaryKey,
            SecondaryKey = keys.SecondaryKey,
            CreatedDate = subscription.Properties.CreatedDate
        };
    }

    public async Task<List<SubscriptionInfo>> GetSubscriptionsByEmailAsync(string? email)
    {
        _logger.LogInformation("Fetching subscriptions from APIM for email: {Email}", email ?? "all");
        
        var apimSubscriptions = await _apimClient.ListAllSubscriptionsAsync();
        
        var subscriptions = new List<SubscriptionInfo>();
        
        // Look up the actual user ID by email if provided
        string? expectedOwnerId = null;
        if (!string.IsNullOrEmpty(email))
        {
            var user = await _apimClient.GetUserByEmailAsync(email);
            if (user != null)
            {
                expectedOwnerId = user.Id;
                _logger.LogInformation("Found user ID: {OwnerId} for email: {Email}", expectedOwnerId, email);
            }
            else
            {
                _logger.LogWarning("No user found for email: {Email}", email);
                return subscriptions; // Return empty list if user not found
            }
        }
        
        foreach (var sub in apimSubscriptions.Value ?? Array.Empty<ApimSubscriptionResponse>())
        {
            var productId = ExtractProductIdFromScope(sub.Properties?.Scope);
            
            var subscriptionInfo = new SubscriptionInfo
            {
                SubscriptionId = sub.Name!,
                SubscriptionName = sub.Properties!.DisplayName!,
                State = sub.Properties.State!,
                ProductId = productId,
                ProductName = productId,
                PrimaryKey = null,
                SecondaryKey = null,
                CreatedDate = sub.Properties.CreatedDate
            };
            
            // Filter by ownerId if email provided
            if (string.IsNullOrEmpty(email) || 
                sub.Properties.OwnerId?.Equals(expectedOwnerId, StringComparison.OrdinalIgnoreCase) == true)
            {
                subscriptions.Add(subscriptionInfo);
                _logger.LogInformation("Including subscription {SubscriptionId} - Owner: {OwnerId}", 
                    sub.Name, sub.Properties.OwnerId ?? "none");
            }
        }
        
        _logger.LogInformation("Retrieved {Count} subscriptions from APIM for email {Email}", 
            subscriptions.Count, email ?? "all");
        
        return subscriptions;
    }

    public async Task<SubscriptionInfo> UpdateSubscriptionAsync(string subscriptionId, string action)
    {
        _logger.LogInformation("Updating subscription {SubscriptionId} - Action: {Action}", 
            subscriptionId, action);

        var newState = action.ToLower() switch
        {
            "activate" => "active",
            "suspend" => "suspended",
            "cancel" => "cancelled",
            _ => throw new ArgumentException($"Invalid action: {action}")
        };

        await _apimClient.UpdateSubscriptionStateAsync(subscriptionId, newState);

        return await GetSubscriptionInfoAsync(subscriptionId);
    }

    public async Task RotateKeyAsync(string subscriptionId, string keyType)
    {
        _logger.LogInformation("Rotating {KeyType} key for subscription {SubscriptionId}", 
            keyType, subscriptionId);

        if (keyType.Equals("primary", StringComparison.OrdinalIgnoreCase))
        {
            await _apimClient.RegeneratePrimaryKeyAsync(subscriptionId);
        }
        else if (keyType.Equals("secondary", StringComparison.OrdinalIgnoreCase))
        {
            await _apimClient.RegenerateSecondaryKeyAsync(subscriptionId);
        }
        else
        {
            throw new ArgumentException($"Invalid key type: {keyType}");
        }
    }

    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        _logger.LogInformation("Cancelling subscription: {SubscriptionId}", subscriptionId);

        await _apimClient.DeleteSubscriptionAsync(subscriptionId);
    }

    private string GenerateSubscriptionName(string email, string productId)
    {
        // Create a unique subscription name
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var emailPrefix = email.Split('@')[0];
        return $"{productId}-{emailPrefix}-{timestamp}".ToLower();
    }

    private string ExtractProductIdFromScope(string? scope)
    {
        if (string.IsNullOrEmpty(scope))
            return "unknown";

        // Scope format: .../products/{productId}
        var parts = scope.Split('/');
        var productIndex = Array.IndexOf(parts, "products");
        
        return (productIndex >= 0 && productIndex < parts.Length - 1) 
            ? parts[productIndex + 1] 
            : "unknown";
    }
}
