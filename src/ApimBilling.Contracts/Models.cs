namespace ApimBilling.Contracts;

/// <summary>
/// Represents an API product from APIM
/// </summary>
public record Product
{
    public required string ProductId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? State { get; init; }
    public bool SubscriptionRequired { get; init; }
}

/// <summary>
/// Request to purchase a product
/// </summary>
public record PurchaseRequest
{
    public required string ProductId { get; init; }
    public required string CustomerEmail { get; init; }
    public required string CustomerName { get; init; }
}

/// <summary>
/// Response after successful purchase
/// </summary>
public record PurchaseResponse
{
    public required string SubscriptionId { get; init; }
    public required string SubscriptionName { get; init; }
    public required string PrimaryKey { get; init; }
    public required string SecondaryKey { get; init; }
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required string State { get; init; }
    public required DateTime CreatedDate { get; init; }
}

/// <summary>
/// Subscription status information
/// </summary>
public record SubscriptionInfo
{
    public required string SubscriptionId { get; init; }
    public required string SubscriptionName { get; init; }
    public required string State { get; init; }
    public required string ProductId { get; init; }
    public string? ProductName { get; init; }
    public string? PrimaryKey { get; init; }
    public string? SecondaryKey { get; init; }
    public DateTime CreatedDate { get; init; }
}

/// <summary>
/// Request to update subscription state
/// </summary>
public record UpdateSubscriptionRequest
{
    public required string Action { get; init; } // "activate", "suspend", "cancel"
}

/// <summary>
/// Request to rotate subscription keys
/// </summary>
public record RotateKeyRequest
{
    public required string KeyType { get; init; } // "primary" or "secondary"
}
