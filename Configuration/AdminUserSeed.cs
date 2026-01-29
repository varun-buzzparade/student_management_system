using StudentManagementSystem.Models;

namespace StudentManagementSystem.Configuration;

/// <summary>
/// Admin user entry for seeding. Email and Password required; others optional (defaults applied).
/// </summary>
public sealed class AdminUserSeed
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int? Age { get; set; }
    public decimal? HeightCm { get; set; }
    public Gender? Gender { get; set; }
    public string? MobileNumber { get; set; }
}
