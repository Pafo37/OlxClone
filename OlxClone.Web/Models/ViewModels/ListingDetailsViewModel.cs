namespace OlxClone.Web.Models.ViewModels;

public class ListingDetailsViewModel
{
    public Listing Listing { get; set; } = null!;
    public bool IsFavorite { get; set; }
    public bool IsOwner { get; set; }
    public bool CanModerate { get; set; }
}
