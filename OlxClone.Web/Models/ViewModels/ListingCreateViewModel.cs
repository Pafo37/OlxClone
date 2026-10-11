using System.ComponentModel.DataAnnotations;

namespace OlxClone.Web.Models.ViewModels;

public class ListingCreateViewModel
{
    [Required(ErrorMessage = "Заглавието е задължително.")]
    [StringLength(200, ErrorMessage = "Заглавието не може да е повече от 200 символа.")]
    [Display(Name = "Заглавие")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Описанието е задължително.")]
    [StringLength(4000, ErrorMessage = "Описанието не може да е повече от 4000 символа.")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефонният номер е задължителен.")]
    [Phone(ErrorMessage = "Невалиден телефонен номер.")]
    [StringLength(30)]
    [Display(Name = "Телефонен номер")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Снимки")]
    public List<IFormFile>? Images { get; set; }
}
