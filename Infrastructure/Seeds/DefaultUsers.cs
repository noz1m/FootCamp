using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeds;

public class DefaultUsers
{
    public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // ===== ADMIN =====
        var admin = await userManager.FindByEmailAsync("nozimqoziev@gmail.com");
        if (admin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "nozimqoziev@gmail.com",
                Email = "nozimqoziev@gmail.com",
                FullName = "Nozim Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(adminUser, "Nozimjanov@2005");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // ===== MANAGER =====
        var manager = await userManager.FindByEmailAsync("nozimqcsgo@gmail.com");
        if (manager == null)
        {
            var managerUser = new ApplicationUser
            {
                UserName = "nozimqcsgo@gmail.com",
                Email = "nozimcsgo@gmail.com",
                FullName = "Nozim Manager",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(managerUser, "Nozimjanov@2005");
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }
    }
}