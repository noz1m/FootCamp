using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeds;

public class DefaultRoles
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new List<string>
        {
            "Admin",
            "Manager",
            "User"
        };

        foreach (var role in roles)
        {
            var existing = await roleManager.FindByNameAsync(role);
            if (existing != null) 
                continue;
            
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}