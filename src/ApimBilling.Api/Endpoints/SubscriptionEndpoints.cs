using ApimBilling.Api.Services;
using ApimBilling.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ApimBilling.Api.Endpoints;

public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions")
            .WithTags("Subscriptions");

        group.MapGet("/", GetAllSubscriptions)
            .WithName("GetAllSubscriptions")
            .WithSummary("Get all subscriptions")
            .WithDescription("Required headers: X-APIM-ServiceName, X-APIM-ResourceGroup");

        group.MapPost("/purchase", PurchaseProduct)
            .WithName("PurchaseProduct")
            .WithSummary("Purchase a product and create APIM subscription")
            .WithDescription("Required headers: X-APIM-ServiceName, X-APIM-ResourceGroup");

        group.MapGet("/{subscriptionId}", GetSubscription)
            .WithName("GetSubscription")
            .WithSummary("Get subscription details")
            .WithDescription("Required headers: X-APIM-ServiceName, X-APIM-ResourceGroup");

        group.MapPatch("/{subscriptionId}/state", UpdateSubscriptionState)
            .WithName("UpdateSubscriptionState")
            .WithSummary("Update subscription state (activate/suspend/cancel)")
            .WithDescription("Required headers: X-APIM-ServiceName, X-APIM-ResourceGroup");

        group.MapPost("/{subscriptionId}/rotate-key", RotateKey)
            .WithName("RotateKey")
            .WithSummary("Rotate subscription key")
            .WithDescription("Required headers: X-APIM-ServiceName, X-APIM-ResourceGroup");

        group.MapDelete("/{subscriptionId}", CancelSubscription)
            .WithName("CancelSubscription")
            .WithSummary("Cancel and delete subscription")
            .WithDescription("Required headers: X-APIM-ServiceName, X-APIM-ResourceGroup");
    }

    private static async Task<IResult> GetAllSubscriptions(
        [FromQuery] string? email,
        [FromServices] IBillingService billingService)
    {
        try
        {
            var subscriptions = await billingService.GetSubscriptionsByEmailAsync(email);
            return Results.Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to retrieve subscriptions",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> PurchaseProduct(
        [FromBody] PurchaseRequest request,
        [FromServices] IBillingService billingService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Purchase request received for product: {ProductId}", request.ProductId);
            var result = await billingService.ProcessPurchaseAsync(request);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process purchase");
            return Results.Problem(
                title: "Purchase Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetSubscription(
        string subscriptionId,
        [FromServices] IBillingService billingService)
    {
        try
        {
            var subscription = await billingService.GetSubscriptionInfoAsync(subscriptionId);
            return Results.Ok(subscription);
        }
        catch (Exception ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateSubscriptionState(
        string subscriptionId,
        [FromBody] UpdateSubscriptionRequest request,
        [FromServices] IBillingService billingService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Update subscription {SubscriptionId} - Action: {Action}", 
                subscriptionId, request.Action);
            
            var result = await billingService.UpdateSubscriptionAsync(subscriptionId, request.Action);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update subscription state");
            return Results.Problem(
                title: "Update Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> RotateKey(
        string subscriptionId,
        [FromBody] RotateKeyRequest request,
        [FromServices] IBillingService billingService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Rotate {KeyType} key for subscription {SubscriptionId}", 
                request.KeyType, subscriptionId);
            
            await billingService.RotateKeyAsync(subscriptionId, request.KeyType);
            
            // Get updated subscription with new key
            var subscription = await billingService.GetSubscriptionInfoAsync(subscriptionId);
            return Results.Ok(subscription);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rotate key");
            return Results.Problem(
                title: "Key Rotation Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CancelSubscription(
        string subscriptionId,
        [FromServices] IBillingService billingService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Cancel subscription: {SubscriptionId}", subscriptionId);
            await billingService.CancelSubscriptionAsync(subscriptionId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel subscription");
            return Results.Problem(
                title: "Cancellation Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}
