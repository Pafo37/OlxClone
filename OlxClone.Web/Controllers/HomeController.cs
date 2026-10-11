using Microsoft.AspNetCore.Mvc;
using OlxClone.Web.Models.ViewModels;

namespace OlxClone.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Listings");

        return RedirectToAction("Login", "Account");
    }

    public IActionResult Error()
    {
        var viewModel = new ErrorViewModel { RequestId = HttpContext.TraceIdentifier };
        return View(viewModel);
    }
}
