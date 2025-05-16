using Microsoft.AspNetCore.Identity;
using WebApp.Identity;
using WebApp.Protos;
namespace WebApp.Services;


public interface IAccountService
{
    Task<IdentityResult> CreateAccountAsync(AppUser user, string password);
    Task<AppUser?> FindByEmailAsync(string email);
    Task<string> GetUserNameAsync(string userId);
    Task<SignInResult> LoginAsync(AppUser user, string password);
    Task SignOutAsync();
}
public class AccountService(SignInManager<AppUser> signInManager, UserProfileProtoService.UserProfileProtoServiceClient client) : IAccountService
{
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly UserProfileProtoService.UserProfileProtoServiceClient _client = client;

    public async Task<AppUser?> FindByEmailAsync(string email)
    {
        var result = await _signInManager.UserManager.FindByEmailAsync(email);
        return result;
    }

    public async Task<IdentityResult> CreateAccountAsync(AppUser user, string password)
    {
        var result = await _signInManager.UserManager.CreateAsync(user, password);
        return result;
    }

    public async Task<SignInResult> LoginAsync(AppUser user, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
        return result;
    }

    public async Task<IdentityResult> DeleteAccountAsync(AppUser user)
    {
        var result = await _signInManager.UserManager.DeleteAsync(user);
        return result;
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        var result = _client.getUserProfileByAppUserId(new getUserProfileByAppUserIdRequest() { AppUserId = userId });
        var userName = result.FirstName + " " + result.LastName;
        return userName;
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
