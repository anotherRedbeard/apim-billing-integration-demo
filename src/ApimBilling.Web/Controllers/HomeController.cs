using Microsoft.AspNetCore.Mvc;
using ApimBilling.Web.Models;

namespace ApimBilling.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // If already logged in, redirect to products
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.UserEmail)))
        {
            return RedirectToAction("Index", "Products");
        }

        return View();
    }

    [HttpPost]
    public IActionResult SetUser(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Please provide email and name.";
            return RedirectToAction("Index");
        }

        // Store user info in session
        HttpContext.Session.SetString(SessionKeys.UserEmail, email);
        HttpContext.Session.SetString(SessionKeys.UserName, name);

        TempData["Success"] = $"Welcome, {name}!";
        
        // Redirect to APIM configuration page
        return RedirectToAction("ConfigureApim");
    }

    public IActionResult ConfigureApim()
    {
        // Ensure user is logged in
        if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.UserEmail)))
        {
            return RedirectToAction("Index");
        }

        var model = new ApimInstance
        {
            ServiceName = HttpContext.Session.GetString(SessionKeys.ApimServiceName) ?? string.Empty,
            ResourceGroup = HttpContext.Session.GetString(SessionKeys.ApimResourceGroup) ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult SaveApimConfig(string serviceName, string resourceGroup)
    {
        if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(resourceGroup))
        {
            TempData["Error"] = "Please provide both APIM service name and resource group.";
            return RedirectToAction("ConfigureApim");
        }

        // Store APIM config in session
        HttpContext.Session.SetString(SessionKeys.ApimServiceName, serviceName);
        HttpContext.Session.SetString(SessionKeys.ApimResourceGroup, resourceGroup);

        TempData["Success"] = $"Connected to APIM: {serviceName} ({resourceGroup})";
        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
