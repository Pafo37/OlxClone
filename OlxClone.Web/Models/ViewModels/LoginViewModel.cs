using System.ComponentModel.DataAnnotations;

namespace OlxClone.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Моля, въведете имейл.")]
    [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете парола.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомни ме")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
