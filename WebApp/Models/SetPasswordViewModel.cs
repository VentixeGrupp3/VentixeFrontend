using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class SetPasswordViewModel
    {
        [Display(Name = "Password", Prompt = "Enter Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Confirm Password", Prompt = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match!")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
