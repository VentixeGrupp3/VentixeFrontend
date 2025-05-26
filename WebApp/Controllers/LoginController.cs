using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Identity;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LoginController(SignInManager<AppUser> signInManager) : Controller
    {
        SignInManager<AppUser> _signInManager = signInManager;

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost("login")]


        public IActionResult Login(LoginViewModel model)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Both fields are required";
                return View(model);
            }
            
            var result = _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false).Result;
       
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }


            return View(model);
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Index", "Home");
        }
        
    
    }
}
