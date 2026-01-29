using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using StudentManagementSystem.Configuration;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data;

/// <summary>
/// Ensures Admin and Student roles exist, then seeds admin users from config (AdminUsers:0, :1, ...
/// or legacy AdminCredentials). Each admin is created via helpers; add new entries in config to add admins.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await EnsureRoleAsync(roleManager, Roles.Admin);
        await EnsureRoleAsync(roleManager, Roles.Student);

        var seeds = AdminSeedHelper.GetAdminUserSeedsFromConfig(configuration);
        foreach (var seed in seeds)
        {
            await AdminSeedHelper.EnsureAdminExistsAsync(userManager, seed);
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
