using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudentManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(30)]
    public string? StudentId { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    [Range(1, 150)]
    public int Age { get; set; }

    [Range(0.0, 300.0)]
    public decimal HeightCm { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
