using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Models;

namespace OlxClone.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Listing> Listings => Set<Listing>();

    public DbSet<ListingImage> ListingImages => Set<ListingImage>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Listing>(entity =>
        {
            entity.Property(listing => listing.Title).IsRequired().HasMaxLength(200);
            entity.Property(listing => listing.Description).IsRequired().HasMaxLength(4000);
            entity.Property(listing => listing.PhoneNumber).IsRequired().HasMaxLength(30);

            entity.HasIndex(listing => listing.Title);
            entity.HasIndex(listing => listing.Status);

            entity.HasOne(listing => listing.User)
                .WithMany(user => user.Listings)
                .HasForeignKey(listing => listing.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ListingImage>(entity =>
        {
            entity.Property(image => image.FileName).IsRequired();

            entity.HasOne(image => image.Listing)
                .WithMany(listing => listing.Images)
                .HasForeignKey(image => image.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(favorite => new { favorite.UserId, favorite.ListingId }).IsUnique();

            entity.HasOne(favorite => favorite.User)
                .WithMany(user => user.Favorites)
                .HasForeignKey(favorite => favorite.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(favorite => favorite.Listing)
                .WithMany(listing => listing.Favorites)
                .HasForeignKey(favorite => favorite.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}