using System.Text.Json.Serialization;

namespace ApimBilling.Api.Models;

/// <summary>
/// ARM API request to create/update APIM subscription
/// </summary>
public class ApimSubscriptionRequest
{
    [JsonPropertyName("properties")]
    public required SubscriptionProperties Properties { get; set; }
}

public class SubscriptionProperties
{
    [JsonPropertyName("scope")]
    public required string Scope { get; set; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "active";

    [JsonPropertyName("allowTracing")]
    public bool AllowTracing { get; set; } = true;
}

/// <summary>
/// ARM API response for APIM subscription
/// </summary>
public class ApimSubscriptionResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("properties")]
    public SubscriptionResponseProperties? Properties { get; set; }
}

public class SubscriptionResponseProperties
{
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("primaryKey")]
    public string? PrimaryKey { get; set; }

    [JsonPropertyName("secondaryKey")]
    public string? SecondaryKey { get; set; }
}

/// <summary>
/// ARM API response for subscription keys
/// </summary>
public class SubscriptionKeysResponse
{
    [JsonPropertyName("primaryKey")]
    public string? PrimaryKey { get; set; }

    [JsonPropertyName("secondaryKey")]
    public string? SecondaryKey { get; set; }
}

/// <summary>
/// ARM API response for product list
/// </summary>
public class ApimProductListResponse
{
    [JsonPropertyName("value")]
    public ApimProductResponse[]? Value { get; set; }
}

/// <summary>
/// ARM API response for subscription list
/// </summary>
public class ApimSubscriptionListResponse
{
    [JsonPropertyName("value")]
    public ApimSubscriptionResponse[]? Value { get; set; }
}

public class ApimProductResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("properties")]
    public ProductProperties? Properties { get; set; }
}

public class ProductProperties
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("subscriptionRequired")]
    public bool SubscriptionRequired { get; set; }

    [JsonPropertyName("approvalRequired")]
    public bool ApprovalRequired { get; set; }
}

/// <summary>
/// ARM API request to create/update APIM user
/// </summary>
public class ApimUserRequest
{
    [JsonPropertyName("properties")]
    public required UserProperties Properties { get; set; }
}

public class UserProperties
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "active";
}

/// <summary>
/// ARM API response for APIM user
/// </summary>
public class ApimUserResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("properties")]
    public UserResponseProperties? Properties { get; set; }
}

/// <summary>
/// ARM API response for user list
/// </summary>
public class ApimUserListResponse
{
    [JsonPropertyName("value")]
    public ApimUserResponse[]? Value { get; set; }
}

public class UserResponseProperties
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}
