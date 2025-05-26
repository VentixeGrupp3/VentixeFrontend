using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class HomeController : Controller
{

    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

}
