using System.ComponentModel.DataAnnotations;

namespace OlxClone.Web.Models;

public class Listing
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    public ListingStatus Status { get; set; } = ListingStatus.Pending;

    [StringLength(1000)]
    public string? RejectionReason { get; set; }

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
