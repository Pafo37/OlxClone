using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Data;
using OlxClone.Web.Models;

namespace OlxClone.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        var listings = await db.Listings
            .Where(listing => listing.Status == ListingStatus.Pending)
            .Include(listing => listing.Images)
            .Include(listing => listing.User)
            .OrderBy(listing => listing.CreatedAt)
            .ToListAsync();

        return View(listings);
    }

    [HttpGet]
    public async Task<IActionResult> All(string? statusFilter)
    {
        var query = db.Listings.Include(listing => listing.Images).Include(listing => listing.User).AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ListingStatus>(statusFilter, out var status))
            query = query.Where(listing => listing.Status == status);

        var listings = await query.OrderByDescending(listing => listing.CreatedAt).ToListAsync();

        ViewBag.StatusFilter = statusFilter;
        return View(listings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        listing.Status = ListingStatus.Approved;
        listing.RejectionReason = null;
        await db.SaveChangesAsync();

        TempData["Success"] = $"Обявата \"{listing.Title}\" беше одобрена.";
        return RedirectToAction(nameof(Pending));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Не отговаря на изискванията." : reason.Trim();
        await db.SaveChangesAsync();

        TempData["Success"] = $"Обявата \"{listing.Title}\" беше отказана.";
        return RedirectToAction(nameof(Pending));
    }
}