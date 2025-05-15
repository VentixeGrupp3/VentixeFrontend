using Microsoft.AspNetCore.Identity;
using WebApp.Identity;
namespace WebApp.Services;


public interface IAccountService
{
    Task<IdentityResult> CreateAccountAsync(AppUser user, string password);
    Task<AppUser?> FindByEmailAsync(string email);
    Task<SignInResult> LoginAsync(AppUser user, string password);
    Task SignOutAsync();
}
public class AccountService(SignInManager<AppUser> signInManager) : IAccountService
{
    private readonly SignInManager<AppUser> _signInManager = signInManager;

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
        _signInManager.UserManager.GetRolesAsync(user);
        return result;
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
