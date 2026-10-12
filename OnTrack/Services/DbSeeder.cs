using Microsoft.AspNetCore.Identity;
using OnTrack.Models;

namespace OnTrack.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Roles
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. Remove the old Staff role if it exists (it fails if users still have it)
        var staff = await roleManager.FindByNameAsync("Staff");
        if (staff != null)
            await roleManager.DeleteAsync(staff);

        // 3. First admin account
        const string adminEmail = "admin@trains.com";
        const string adminPassword = "Admin@12345";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",   // NEW
                EmailConfirmed = true   // so the admin can log in without the email step
                // if ApplicationUser has required properties (FullName, etc.), set them here
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
                return;   // password rules failed or a required property is missing
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}