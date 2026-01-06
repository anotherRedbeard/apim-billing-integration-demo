using ApimBilling.Contracts;
using ApimBilling.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApimBilling.Web.Controllers;

public class SubscriptionsController : Controller
{
    private readonly IBillingApiClient _billingApi;
    private readonly ILogger<SubscriptionsController> _logger;
    private const string SessionKeyEmail = "UserEmail";
    private const string SessionKeyName = "UserName";

    public SubscriptionsController(IBillingApiClient billingApi, ILogger<SubscriptionsController> logger)
    {
        _billingApi = billingApi;
        _logger = logger;
    }

    public async Task<IActionResult> MySubscriptions()
    {
        var email = HttpContext.Session.GetString(SessionKeyEmail);
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var subscriptions = await _billingApi.GetSubscriptionsByEmailAsync(email);
            
            var orderedSubscriptions = subscriptions
                .OrderByDescending(s => s.CreatedDate)
                .ToList();

            return View(orderedSubscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load subscriptions for {Email}", email);
            TempData["Error"] = "Failed to load subscriptions. Please try again.";
            return View(new List<SubscriptionInfo>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Purchase(string productId, string productName)
    {
        var email = HttpContext.Session.GetString(SessionKeyEmail);
        var name = HttpContext.Session.GetString(SessionKeyName);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var request = new PurchaseRequest
            {
                ProductId = productId,
                CustomerEmail = email,
                CustomerName = name
            };

            var result = await _billingApi.PurchaseProductAsync(request);
            
            TempData["Success"] = $"Successfully purchased {productName}! Your subscription is ready.";
            
            return RedirectToAction("Details", new { id = result.SubscriptionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purchase product {ProductId}", productId);
            TempData["Error"] = $"Failed to purchase {productName}. Please try again.";
            return RedirectToAction("Index", "Products");
        }
    }

    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var subscription = await _billingApi.GetSubscriptionAsync(id);
            return View(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load subscription {SubscriptionId}", id);
            TempData["Error"] = "Subscription not found.";
            return RedirectToAction("Index", "Products");
        }
    }

    [HttpPost]
    public async Task<IActionResult> StopPaying(string id)
    {
        try
        {
            await _billingApi.UpdateSubscriptionStateAsync(id, "suspend");
            TempData["Success"] = "Subscription suspended due to non-payment.";
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suspend subscription {SubscriptionId}", id);
            TempData["Error"] = "Failed to suspend subscription. Please try again.";
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ResumePaying(string id)
    {
        try
        {
            await _billingApi.UpdateSubscriptionStateAsync(id, "activate");
            TempData["Success"] = "Subscription reactivated!";
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate subscription {SubscriptionId}", id);
            TempData["Error"] = "Failed to activate subscription. Please try again.";
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeclineToPay(string id)
    {
        try
        {
            await _billingApi.DeleteSubscriptionAsync(id);
            TempData["Success"] = "Subscription cancelled and deleted.";
            return RedirectToAction("Index", "Products");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete subscription {SubscriptionId}", id);
            TempData["Error"] = "Failed to cancel subscription. Please try again.";
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RotateKey(string id, string keyType)
    {
        try
        {
            await _billingApi.RotateKeyAsync(id, keyType);
            TempData["Success"] = $"{keyType} key rotated successfully!";
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate {KeyType} key for subscription {SubscriptionId}", keyType, id);
            TempData["Error"] = $"Failed to rotate {keyType} key. Please try again.";
            return RedirectToAction("Details", new { id });
        }
    }
}
