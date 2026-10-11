namespace OlxClone.Web.Models.ViewModels;

public class ListingCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFavorite { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}

public class ListingIndexViewModel
{
    public List<ListingCardViewModel> Listings { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}
