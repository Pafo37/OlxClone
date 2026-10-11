using Microsoft.AspNetCore.Mvc;
using OlxClone.Web.Models.ViewModels;

namespace OlxClone.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Listings") : RedirectToAction("Login", "Account");
    }

    public IActionResult Error()
    {
        var viewModel = new ErrorViewModel { RequestId = HttpContext.TraceIdentifier };
        return View(viewModel);
    }
}
