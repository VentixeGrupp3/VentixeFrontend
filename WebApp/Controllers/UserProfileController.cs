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
            var model = _client.getUserProfileByAppUserIdAsync(new getUserProfileByAppUserIdRequest()
            {
                AppUserId = userId

            });
            
            if (model != null)
            {
               var profile = new ProfileInformationViewModel()
                {
                    FirstName = model.ResponseAsync.Result.FirstName,
                    LastName = model.ResponseAsync.Result.LastName,
                    StreetName = model.ResponseAsync.Result.StreetName,
                    City = model.ResponseAsync.Result.City,
                    PhoneNumber = model.ResponseAsync.Result.PhoneNumber,
                    PostalCode = model.ResponseAsync.Result.PostalCode
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
