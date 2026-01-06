using ApimBilling.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApimBilling.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IBillingApiClient _billingApi;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IBillingApiClient billingApi, ILogger<ProductsController> logger)
    {
        _billingApi = billingApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await _billingApi.GetProductsAsync();
            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load products");
            TempData["Error"] = "Failed to load products. Please try again later.";
            return View(new List<ApimBilling.Contracts.Product>());
        }
    }
}
