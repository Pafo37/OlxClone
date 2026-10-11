using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Data;
using OlxClone.Web.Models;

namespace OlxClone.Web.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var listings = await _db.Listings
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
        var userId = _userManager.GetUserId(User)!;
        var existing = await _db.Favorites.FirstOrDefaultAsync(favorite => favorite.UserId == userId && favorite.ListingId == listingId);

        if (existing != null)
        {
            _db.Favorites.Remove(existing);
        }
        else
        {
            var listingExists = await _db.Listings.AnyAsync(listing => listing.Id == listingId);
            if (!listingExists)
                return NotFound();

            _db.Favorites.Add(new Favorite { UserId = userId, ListingId = listingId, CreatedAt = DateTime.UtcNow });
        }

        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}
