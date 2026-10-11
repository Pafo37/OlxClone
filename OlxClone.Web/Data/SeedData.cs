using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OlxClone.Web.Models;

namespace OlxClone.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = config["AdminSeed:Email"] ?? "admin@olxclone.local";
        var adminPassword = config["AdminSeed:Password"] ?? "Admin123!";
        var adminFullName = config["AdminSeed:FullName"] ?? "Administrator";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminFullName,
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
