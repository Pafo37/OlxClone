using Microsoft.AspNetCore.Identity;

namespace OlxClone.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
