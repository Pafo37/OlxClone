using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Data;
using OlxClone.Web.Models;
using OlxClone.Web.Models.ViewModels;

namespace OlxClone.Web.Controllers;

[Authorize]
public class ListingsController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment)
    : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxImageBytes = 5 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var userId = userManager.GetUserId(User);

        var query = db.Listings.Where(l => l.Status == ListingStatus.Approved).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(l => EF.Functions.Like(l.Title, $"%{term}%"));
        }

        var favoriteIds = await db.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.ListingId)
            .ToListAsync();

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Include(l => l.Images)
            .Include(l => l.User)
            .ToListAsync();

        var vm = new ListingIndexViewModel
        {
            SearchTerm = search,
            Listings = listings.Select(l => new ListingCardViewModel
            {
                Id = l.Id,
                Title = l.Title,
                CoverImageUrl = BuildCoverImageUrl(l),
                ViewCount = l.ViewCount,
                CreatedAt = l.CreatedAt,
                IsFavorite = favoriteIds.Contains(l.Id)
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var listing = await db.Listings
            .Include(listing => listing.Images)
            .Include(listing => listing.User)
            .FirstOrDefaultAsync(listing => listing.Id == id);

        if (listing is null)
            return NotFound();

        var userId = userManager.GetUserId(User);
        var isOwner = listing.UserId == userId;
        var isAdmin = User.IsInRole("Admin");

        if (listing.Status != ListingStatus.Approved && !isOwner && !isAdmin)
            return NotFound();

        listing.ViewCount++;
        await db.SaveChangesAsync();

        var isFavorite = await db.Favorites.AnyAsync(f => f.UserId == userId && f.ListingId == id);

        var vm = new ListingDetailsViewModel
        {
            Listing = listing,
            IsFavorite = isFavorite,
            IsOwner = isOwner,
            CanModerate = isAdmin
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create() => View(new ListingCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ListingCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = userManager.GetUserId(User)!;

        var listing = new Listing
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Status = ListingStatus.Pending,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        db.Listings.Add(listing);
        await db.SaveChangesAsync();

        if (model.Images is { Count: > 0 })
        {
            await SaveImagesAsync(listing.Id, model.Images);
            await db.SaveChangesAsync();
        }

        TempData["Success"] = "Обявата е изпратена за одобрение от администратор.";
        return RedirectToAction(nameof(MyListings));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = userManager.GetUserId(User);
        var listing = await db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);

        if (listing is null)
            return NotFound();

        if (listing.UserId != userId)
            return Forbid();

        var viewModel = new ListingEditViewModel
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            PhoneNumber = listing.PhoneNumber,
            ExistingImages = listing.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new ExistingImageViewModel { Id = i.Id, Url = BuildImageUrl(listing.Id, i.FileName) })
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ListingEditViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var userId = userManager.GetUserId(User);
        var listing = await db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);

        if (listing is null)
            return NotFound();

        if (listing.UserId != userId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            model.ExistingImages = listing.Images
                .OrderBy(image => image.SortOrder)
                .Select(image => new ExistingImageViewModel { Id = image.Id, Url = BuildImageUrl(listing.Id, image.FileName) })
                .ToList();
            return View(model);
        }

        listing.Title = model.Title.Trim();
        listing.Description = model.Description.Trim();
        listing.PhoneNumber = model.PhoneNumber.Trim();

        listing.Status = ListingStatus.Pending;
        listing.RejectionReason = null;

        if (model.ImageIdsToDelete is { Count: > 0 })
        {
            var toDelete = listing.Images.Where(i => model.ImageIdsToDelete.Contains(i.Id)).ToList();
            foreach (var img in toDelete)
            {
                DeleteImageFile(listing.Id, img.FileName);
                db.ListingImages.Remove(img);
            }
        }

        if (model.NewImages is { Count: > 0 })
        {
            await SaveImagesAsync(listing.Id, model.NewImages);
        }

        await db.SaveChangesAsync();

        TempData["Success"] = "Обявата е обновена и изпратена за повторно одобрение.";
        return RedirectToAction(nameof(MyListings));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        var listing = await db.Listings.Include(l => l.Images).Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
        if (listing is null)
            return NotFound();

        if (listing.UserId != userId && !isAdmin)
            return Forbid();

        return View(listing);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        var listing = await db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
        if (listing is null)
            return NotFound();

        if (listing.UserId != userId && !isAdmin)
            return Forbid();

        var dir = Path.Combine(environment.WebRootPath, "uploads", "listings", listing.Id.ToString());
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);

        db.Listings.Remove(listing);
        await db.SaveChangesAsync();

        TempData["Success"] = "Обявата беше изтрита.";
        return RedirectToAction(nameof(MyListings));
    }

    [HttpGet]
    public async Task<IActionResult> MyListings()
    {
        var userId = userManager.GetUserId(User);

        var listings = await db.Listings
            .Where(listing => listing.UserId == userId)
            .Include(listing => listing.Images)
            .OrderByDescending(listing => listing.CreatedAt)
            .ToListAsync();

        return View(listings);
    }

    private string? BuildCoverImageUrl(Listing listing)
    {
        var cover = listing.Images.OrderBy(i => i.SortOrder).FirstOrDefault();
        return cover is null ? null : BuildImageUrl(listing.Id, cover.FileName);
    }

    private static string BuildImageUrl(int listingId, string fileName) => $"/uploads/listings/{listingId}/{fileName}";

    private async Task SaveImagesAsync(int listingId, List<IFormFile> files)
    {
        var dir = Path.Combine(environment.WebRootPath, "uploads", "listings", listingId.ToString());
        Directory.CreateDirectory(dir);

        var order = await db.ListingImages.CountAsync(i => i.ListingId == listingId);

        foreach (var file in files)
        {
            if (file.Length == 0 || file.Length > MaxImageBytes)
                continue;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                continue;

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(dir, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            db.ListingImages.Add(new ListingImage
            {
                ListingId = listingId,
                FileName = fileName,
                SortOrder = order++
            });
        }
    }

    private void DeleteImageFile(int listingId, string fileName)
    {
        var path = Path.Combine(environment.WebRootPath, "uploads", "listings", listingId.ToString(), fileName);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }
}
