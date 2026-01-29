using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

/// <summary>
/// Shared properties for admin student list items and edit view models.
/// </summary>
public abstract class AdminStudentBaseViewModel
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Age")]
    public int Age { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    [MaxLength(20)]
    [Display(Name = "Mobile Number")]
    public string? MobileNumber { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}
