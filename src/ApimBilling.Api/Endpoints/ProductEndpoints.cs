using ApimBilling.Api.Services;
using ApimBilling.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ApimBilling.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get all available API products from APIM");
    }

    private static async Task<IResult> GetProducts(
        [FromServices] IBillingService billingService)
    {
        var products = await billingService.GetProductsAsync();
        return Results.Ok(products);
    }
}
