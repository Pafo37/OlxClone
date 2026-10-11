using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Data;
using OlxClone.Web.Models;

namespace OlxClone.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        var listings = await _db.Listings
            .Where(l => l.Status == ListingStatus.Pending)
            .Include(l => l.Images)
            .Include(l => l.User)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

        return View(listings);
    }

    [HttpGet]
    public async Task<IActionResult> All(string? statusFilter)
    {
        var query = _db.Listings.Include(l => l.Images).Include(l => l.User).AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ListingStatus>(statusFilter, out var status))
            query = query.Where(l => l.Status == status);

        var listings = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

        ViewBag.StatusFilter = statusFilter;
        return View(listings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        listing.Status = ListingStatus.Approved;
        listing.RejectionReason = null;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Обявата \"{listing.Title}\" беше одобрена.";
        return RedirectToAction(nameof(Pending));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Не отговаря на изискванията." : reason.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Обявата \"{listing.Title}\" беше отказана.";
        return RedirectToAction(nameof(Pending));
    }
}
