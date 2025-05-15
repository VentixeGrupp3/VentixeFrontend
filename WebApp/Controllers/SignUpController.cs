using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Identity;
using WebApp.Models;
using WebApp.Protos;
using WebApp.Services;

namespace WebApp.Controllers;

public class SignUpController(IAccountService accountService, VerificationService verificationService, UserProfileProtoService.UserProfileProtoServiceClient userProfileClient, IConfiguration configuration) : Controller
{
    private readonly IAccountService _accountService = accountService;
    private readonly VerificationService _verificationService = verificationService;
    private readonly UserProfileProtoService.UserProfileProtoServiceClient _userProfileClient = userProfileClient;
    private readonly IConfiguration _configuration = configuration;



    #region Step 1 - Set Email

    [HttpGet("signup")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Index(SignUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = "Invalid email address";
            return View(model);
        }

        var findAccountResponse = await _accountService.FindByEmailAsync(model.Email);
        if (findAccountResponse != null)
        {
            ViewBag.ErrorMessage = "Account already exists";
            return View(model);
        }

        var verificationResponse = await _verificationService.SendEmailAsync(model.Email);
        if (!verificationResponse.Succeeded)
        {
            ViewBag.ErrorMessage = verificationResponse.Message;
            return View(model);
        }

        TempData["Email"] = model.Email;
        return RedirectToAction("AccountVerification");
    }

    #endregion


    #region Step 2 - Verify Email Address

    [HttpGet("account-verification")]
    public IActionResult AccountVerification()
    {
        if (TempData["Email"] == null)
            return RedirectToAction("Index");

        ViewBag.MaskedEmail = MaskEmail(TempData["Email"]!.ToString()!);
        TempData.Keep("Email");

        return View();
    }

    [HttpPost("account-verification")]
    public async Task<IActionResult> AccountVerification(AccountVerificationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = TempData["Email"]?.ToString();

        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction("Index");

        var response = await _verificationService.ConfirmCodeAsync(email, model.Code);
        if (!response.Succeeded)
        {
            ViewBag.ErrorMessage = response.Message;
            TempData.Keep("Email");
            return View(model);
        }

        TempData["Email"] = email;
        return RedirectToAction("SetPassword");
    }

    #endregion


    #region Step 3 - Set Password

    [HttpGet("set-password")]
    public IActionResult SetPassword()
    {
        return View();
    }

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = TempData["Email"]?.ToString();
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Index));

        var account = new AppUser
        {
            UserName = email,
            Email = email,
        };

        var response = await _accountService.CreateAccountAsync(account, model.Password);
        if (!response.Succeeded)
        {
            TempData.Keep("Email");
            return View(model);
        }

        TempData["UserId"] = _accountService.FindByEmailAsync(email).Result!.Id;
        return RedirectToAction("ProfileInformation");
    }

    #endregion


    #region Step 4 - Set Profile Information

    [HttpGet("profile-information")]
    public IActionResult ProfileInformation()
    {
        return View();
    }


    [HttpPost("profile-information")]
    public async Task<IActionResult> ProfileInformationAsync(ProfileInformationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = TempData["UserId"]?.ToString();
        var request = new createUserProfileRequest() { 
            AppUserId = userId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            StreetName = model.StreetName,
            City = model.City,
            PhoneNumber = model.PhoneNumber,
            PostalCode = model.PostalCode,
        };
        var serviceBusClient = new ServiceBusClient(_configuration.GetConnectionString("AzureServiceBus"));
        var sender = serviceBusClient.CreateSender("userprofile");

        var message = new ServiceBusMessage(JsonSerializer.Serialize(request));
        await sender.SendMessageAsync(message);
        return RedirectToAction("Index", "Home");
    }

    #endregion


    private string MaskEmail(string email)
    {
        var parts = email.Split('@');
        var firstChar = parts[0].First();
        return $"{firstChar}*****@{parts[1]}";
    }
}