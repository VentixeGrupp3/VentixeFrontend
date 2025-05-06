using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class SignUpViewModel
{
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email", Prompt = "Enter email address")]
    [Required(ErrorMessage = "Email must be provided.")]
    public string Email { get; set; } = null!;
}
