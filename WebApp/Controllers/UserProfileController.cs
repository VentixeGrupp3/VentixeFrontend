using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Identity;
using WebApp.Models;
using WebApp.Protos;

namespace WebApp.Controllers
{
    public class UserProfileController(UserProfileProtoService.UserProfileProtoServiceClient client, UserManager<AppUser> userManager) : Controller
    {
        private readonly UserProfileProtoService.UserProfileProtoServiceClient _client = client;
        private readonly UserManager<AppUser> _userManager = userManager;

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = await _userManager.GetUserIdAsync(user);
            var model = _client.getUserProfileByAppUserId(new getUserProfileByAppUserIdRequest()
            {
                AppUserId = userId

            });
                
            if (model != null)
            {
               var profile = new ProfileInformationViewModel()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StreetName = model.StreetName,
                    City = model.City,
                    PhoneNumber = model.PhoneNumber,
                    PostalCode = model.PostalCode
                };
                return View(profile);
            }
            else
            {
                return View();
            }

        }
    }
}
