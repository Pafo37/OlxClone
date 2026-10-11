using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Data;
using OlxClone.Web.Models;

namespace OlxClone.Web.Controllers;

[Authorize]
public class FavoritesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);

        var listings = await db.Listings
            .Where(l => l.Favorites.Any(f => f.UserId == userId))
            .Include(listing => listing.Images)
            .Include(listing => listing.User)
            .OrderByDescending(l => l.Favorites
                .Where(favorite => favorite.UserId == userId)
                .Max(favorite => favorite.CreatedAt))
            .ToListAsync();

        return View(listings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int listingId, string? returnUrl = null)
    {
        var userId = userManager.GetUserId(User)!;
        var existing =
            await db.Favorites.FirstOrDefaultAsync(favorite =>
                favorite.UserId == userId && favorite.ListingId == listingId);

        if (existing != null)
        {
            db.Favorites.Remove(existing);
        }
        else
        {
            var listingExists = await db.Listings.AnyAsync(listing => listing.Id == listingId);
            if (!listingExists)
                return NotFound();

            db.Favorites.Add(new Favorite { UserId = userId, ListingId = listingId, CreatedAt = DateTime.UtcNow });
        }

        await db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}