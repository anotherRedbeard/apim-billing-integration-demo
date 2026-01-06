using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ApimBilling.Api.Configuration;
using ApimBilling.Api.Models;
using Azure.Core;
using Azure.Identity;

namespace ApimBilling.Api.Services;

/// <summary>
/// Client for managing APIM subscriptions via Azure Resource Manager REST API
/// </summary>
public interface IApimSubscriptionClient
{
    Task<ApimUserResponse> CreateOrGetUserAsync(string email, string firstName, string lastName);
    Task<ApimUserResponse?> GetUserByEmailAsync(string email);
    Task<ApimSubscriptionResponse> CreateSubscriptionAsync(string subscriptionName, string productId, string displayName, string? ownerId = null);
    Task<SubscriptionKeysResponse> GetSubscriptionKeysAsync(string subscriptionName);
    Task<ApimSubscriptionResponse> GetSubscriptionAsync(string subscriptionName);
    Task<ApimSubscriptionListResponse> ListAllSubscriptionsAsync();
    Task RegeneratePrimaryKeyAsync(string subscriptionName);
    Task RegenerateSecondaryKeyAsync(string subscriptionName);
    Task UpdateSubscriptionStateAsync(string subscriptionName, string state);
    Task DeleteSubscriptionAsync(string subscriptionName);
    Task<ApimProductListResponse> ListApimProductsAsync();
}

public class ApimSubscriptionClient : IApimSubscriptionClient
{
    private readonly HttpClient _httpClient;
    private readonly ApimSettings _settings;
    private readonly TokenCredential _credential;
    private readonly ILogger<ApimSubscriptionClient> _logger;
    private const string ApiVersion = "2024-05-01";

    public ApimSubscriptionClient(
        HttpClient httpClient,
        ApimSettings settings,
        ILogger<ApimSubscriptionClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
        
        // Use Managed Identity in Azure, DefaultAzureCredential for local dev
        _credential = new DefaultAzureCredential();
    }

    public async Task<ApimSubscriptionResponse> CreateSubscriptionAsync(
        string subscriptionName, 
        string productId, 
        string displayName,
        string? ownerId = null)
    {
        var url = BuildSubscriptionUrl(subscriptionName);
        var scope = $"/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.ApiManagement/service/{_settings.ApimName}/products/{productId}";

        var request = new ApimSubscriptionRequest
        {
            Properties = new SubscriptionProperties
            {
                Scope = scope,
                DisplayName = displayName,
                OwnerId = ownerId,
                State = "active"
            }
        };

        var response = await SendRequestAsync<ApimSubscriptionResponse>(
            HttpMethod.Put, 
            url, 
            request);

        _logger.LogInformation("Created APIM subscription: {SubscriptionName} for product: {ProductId}, Owner: {OwnerId}", 
            subscriptionName, productId, ownerId ?? "none");

        return response;
    }

    public async Task<ApimUserResponse> CreateOrGetUserAsync(string email, string firstName, string lastName)
    {
        // First check if user already exists (handles AAD users)
        var existingUser = await GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogInformation("User already exists: {UserId} for email: {Email}", existingUser.Id, email);
            return existingUser;
        }
        
        // Create new user with generated ID
        var userId = email.ToLower().Replace("@", "-at-").Replace(".", "-");
        
        var url = BuildApimResourceUrl($"users/{userId}");
        
        _logger.LogInformation("Creating new APIM user: {UserId} ({Email})", userId, email);
        
        var request = new ApimUserRequest
        {
            Properties = new UserProperties
            {
                Email = email,
                FirstName = firstName,
                LastName = string.IsNullOrWhiteSpace(lastName) ? firstName : lastName
            }
        };

        var response = await SendRequestAsync<ApimUserResponse>(HttpMethod.Put, url, request);
        
        _logger.LogInformation("Created APIM user: {UserId}", userId);
        
        return response;
    }

    public async Task<ApimUserResponse?> GetUserByEmailAsync(string email)
    {
        var url = BuildApimResourceUrl("users");
        
        _logger.LogInformation("Looking up APIM user by email: {Email}", email);
        
        try
        {
            var response = await SendRequestAsync<ApimUserListResponse>(HttpMethod.Get, url);
            
            // Filter users by email in the response properties
            var user = response.Value?.FirstOrDefault(u => 
                string.Equals(u.Properties?.Email, email, StringComparison.OrdinalIgnoreCase));
            
            if (user != null)
            {
                _logger.LogInformation("Found user: {UserId} for email: {Email}", user.Id, email);
            }
            else
            {
                _logger.LogInformation("No user found for email: {Email}", email);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to lookup user by email: {Email}", email);
            return null;
        }
    }

    public async Task<SubscriptionKeysResponse> GetSubscriptionKeysAsync(string subscriptionName)
    {
        var url = BuildSubscriptionUrl(subscriptionName, "/listSecrets");
        
        _logger.LogInformation("Getting subscription keys - URL: {Url}", url);
        
        var response = await SendRequestAsync<SubscriptionKeysResponse>(
            HttpMethod.Post, 
            url);

        _logger.LogInformation("Retrieved keys for subscription: {SubscriptionName}", subscriptionName);

        return response;
    }

    public async Task<ApimSubscriptionResponse> GetSubscriptionAsync(string subscriptionName)
    {
        var url = BuildSubscriptionUrl(subscriptionName);
        
        var response = await SendRequestAsync<ApimSubscriptionResponse>(
            HttpMethod.Get, 
            url);

        return response;
    }

    public async Task RegeneratePrimaryKeyAsync(string subscriptionName)
    {
        var url = BuildSubscriptionUrl(subscriptionName, "/regeneratePrimaryKey");
        
        await SendRequestAsync(HttpMethod.Post, url);

        _logger.LogInformation("Regenerated primary key for subscription: {SubscriptionName}", subscriptionName);
    }

    public async Task RegenerateSecondaryKeyAsync(string subscriptionName)
    {
        var url = BuildSubscriptionUrl(subscriptionName, "/regenerateSecondaryKey");
        
        await SendRequestAsync(HttpMethod.Post, url);

        _logger.LogInformation("Regenerated secondary key for subscription: {SubscriptionName}", subscriptionName);
    }

    public async Task UpdateSubscriptionStateAsync(string subscriptionName, string state)
    {
        var url = BuildSubscriptionUrl(subscriptionName);

        // First get the existing subscription to preserve scope
        var existing = await GetSubscriptionAsync(subscriptionName);
        
        var request = new ApimSubscriptionRequest
        {
            Properties = new SubscriptionProperties
            {
                Scope = existing.Properties!.Scope!,
                DisplayName = existing.Properties.DisplayName!,
                State = state
            }
        };

        await SendRequestAsync<ApimSubscriptionResponse>(HttpMethod.Put, url, request);

        _logger.LogInformation("Updated subscription {SubscriptionName} state to: {State}", 
            subscriptionName, state);
    }

    public async Task DeleteSubscriptionAsync(string subscriptionName)
    {
        var url = BuildSubscriptionUrl(subscriptionName);
        
        await SendRequestAsync(HttpMethod.Delete, url);

        _logger.LogInformation("Deleted subscription: {SubscriptionName}", subscriptionName);
    }

    public async Task<ApimProductListResponse> ListApimProductsAsync()
    {
        var url = BuildApimResourceUrl("products");
        
        var response = await SendRequestAsync<ApimProductListResponse>(HttpMethod.Get, url);

        _logger.LogInformation("Retrieved {Count} products from APIM", response.Value?.Length ?? 0);

        return response;
    }

    public async Task<ApimSubscriptionListResponse> ListAllSubscriptionsAsync()
    {
        var url = BuildApimResourceUrl("subscriptions");
        
        _logger.LogInformation("Listing all subscriptions - URL: {Url}", url);
        
        var response = await SendRequestAsync<ApimSubscriptionListResponse>(HttpMethod.Get, url);

        _logger.LogInformation("Retrieved {Count} subscriptions from APIM", response.Value?.Length ?? 0);

        return response;
    }

    private string BuildSubscriptionUrl(string subscriptionName, string? action = null)
    {
        var baseUrl = $"https://management.azure.com/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.ApiManagement/service/{_settings.ApimName}/subscriptions/{subscriptionName}";
        
        if (!string.IsNullOrEmpty(action))
        {
            baseUrl += action;
        }
        
        return $"{baseUrl}?api-version={ApiVersion}";
    }

    private string BuildApimResourceUrl(string resourceType, string? action = null)
    {
        var baseUrl = $"https://management.azure.com/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.ApiManagement/service/{_settings.ApimName}/{resourceType}";
        
        if (!string.IsNullOrEmpty(action))
        {
            baseUrl += action;
        }
        
        return $"{baseUrl}?api-version={ApiVersion}";
    }

    private async Task<T> SendRequestAsync<T>(HttpMethod method, string url, object? body = null)
    {
        var response = await SendRequestAsync(method, url, body);
        var content = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        }) ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object? body = null)
    {
        var token = await GetAccessTokenAsync();
        
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("ARM API request failed: {StatusCode} - {Error}", 
                response.StatusCode, error);
            response.EnsureSuccessStatusCode();
        }

        return response;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var tokenRequestContext = new TokenRequestContext(
            new[] { "https://management.azure.com/.default" });
        
        var token = await _credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);
        
        return token.Token;
    }
}
