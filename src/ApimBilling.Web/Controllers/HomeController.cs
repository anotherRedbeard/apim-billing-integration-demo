using Microsoft.AspNetCore.Mvc;

namespace ApimBilling.Web.Controllers;

public class HomeController : Controller
{
    private const string SessionKeyEmail = "UserEmail";
    private const string SessionKeyName = "UserName";

    public IActionResult Index()
    {
        // If already logged in, redirect to products
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyEmail)))
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
            TempData["Error"] = "Please provide both email and name.";
            return RedirectToAction("Index");
        }

        HttpContext.Session.SetString(SessionKeyEmail, email);
        HttpContext.Session.SetString(SessionKeyName, name);

        TempData["Success"] = $"Welcome, {name}!";
        return RedirectToAction("MySubscriptions", "Subscriptions");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
