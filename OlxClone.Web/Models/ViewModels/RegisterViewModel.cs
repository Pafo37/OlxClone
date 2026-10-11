using System.ComponentModel.DataAnnotations;

namespace OlxClone.Web.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Моля, въведете име.")]
    [StringLength(150)]
    [Display(Name = "Име и фамилия")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете имейл.")]
    [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете парола.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне {2} символа.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Потвърди парола")]
    [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
