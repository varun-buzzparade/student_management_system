using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using StudentManagementSystem.Configuration;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data;

/// <summary>
/// Helpers for admin seeding: default profile, building ApplicationUser from config, reading admin list.
/// </summary>
public static class AdminSeedHelper
{
    /// <summary>
    /// Default profile when not specified per admin. Change here to affect all new admins.
    /// </summary>
    public static (string FullName, int Age, decimal HeightCm, Gender Gender, string MobileNumber) GetDefaultAdminProfile()
    {
        return ("System Administrator", 30, 170, Gender.Unknown, "0000000000");
    }

    /// <summary>
    /// Builds an ApplicationUser from a seed, using defaults for any missing fields.
    /// </summary>
    public static ApplicationUser BuildApplicationUser(AdminUserSeed seed)
    {
        var (defName, defAge, defHeight, defGender, defMobile) = GetDefaultAdminProfile();
        return new ApplicationUser
        {
            UserName = seed.Email,
            Email = seed.Email,
            FullName = seed.FullName ?? defName,
            Age = seed.Age ?? defAge,
            HeightCm = seed.HeightCm ?? defHeight,
            Gender = seed.Gender ?? defGender,
            MobileNumber = seed.MobileNumber ?? defMobile
        };
    }

    /// <summary>
    /// Reads admin seeds from config. Supports AdminUsers:0, :1, ... and legacy AdminCredentials (single admin).
    /// Env vars: AdminUsers__0__Email, AdminUsers__0__Password, etc., or AdminCredentials__Email / __Password.
    /// </summary>
    public static IReadOnlyList<AdminUserSeed> GetAdminUserSeedsFromConfig(IConfiguration configuration)
    {
        var list = new List<AdminUserSeed>();
        for (var i = 0; i < 100; i++)
        {
            var (seed, endOfList) = ParseSeedAtIndex(configuration, i);
            if (endOfList) break;
            if (seed != null) list.Add(seed);
        }
        if (list.Count == 0)
        {
            var legacy = GetLegacyAdminSeed(configuration);
            if (legacy != null)
                list.Add(legacy);
        }
        return list;
    }

    /// <summary>Returns (seed, endOfList). endOfList=true means no more entries; seed=null with endOfList=false means skip this index (e.g. missing password).</summary>
    private static (AdminUserSeed? Seed, bool EndOfList) ParseSeedAtIndex(IConfiguration configuration, int index)
    {
        var prefix = "AdminUsers:" + index + ":";
        var email = ConfigHelper.GetValue(prefix + "Email", configuration);
        if (string.IsNullOrWhiteSpace(email)) return (null, true);

        var password = ConfigHelper.GetValue(prefix + "Password", configuration);
        if (string.IsNullOrWhiteSpace(password)) return (null, false);

        var fullName = ConfigHelper.GetValue(prefix + "FullName", configuration);
        var ageStr = ConfigHelper.GetValue(prefix + "Age", configuration);
        var heightStr = ConfigHelper.GetValue(prefix + "HeightCm", configuration);
        var genderStr = ConfigHelper.GetValue(prefix + "Gender", configuration);
        var mobile = ConfigHelper.GetValue(prefix + "MobileNumber", configuration);

        var seed = new AdminUserSeed
        {
            Email = email.Trim(),
            Password = password,
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
            Age = int.TryParse(ageStr, out var a) ? a : null,
            HeightCm = decimal.TryParse(heightStr, out var h) ? h : null,
            Gender = Enum.TryParse<Gender>(genderStr, out var g) ? g : null,
            MobileNumber = string.IsNullOrWhiteSpace(mobile) ? null : mobile.Trim()
        };
        return (seed, false);
    }

    private static AdminUserSeed? GetLegacyAdminSeed(IConfiguration configuration)
    {
        var email = ConfigHelper.GetValue("AdminCredentials:Email", configuration);
        var password = ConfigHelper.GetValue("AdminCredentials:Password", configuration);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return null;
        return new AdminUserSeed
        {
            Email = email.Trim(),
            Password = password,
            FullName = ConfigHelper.GetValue("AdminCredentials:FullName", configuration)?.Trim()
        };
    }

    /// <summary>
    /// Ensures the admin from seed exists: create if not, otherwise ensure in Admin role.
    /// Skips if Email or Password is missing.
    /// </summary>
    public static async Task EnsureAdminExistsAsync(UserManager<ApplicationUser> userManager, AdminUserSeed seed)
    {
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
            return;

        var existing = await userManager.FindByEmailAsync(seed.Email);
        if (existing != null)
        {
            if (!await userManager.IsInRoleAsync(existing, Roles.Admin))
                await userManager.AddToRoleAsync(existing, Roles.Admin);
            return;
        }

        var user = BuildApplicationUser(seed);
        var result = await userManager.CreateAsync(user, seed.Password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, Roles.Admin);
    }
}
