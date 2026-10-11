namespace OlxClone.Web.Models;

public class ListingImage
{
    public int Id { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
