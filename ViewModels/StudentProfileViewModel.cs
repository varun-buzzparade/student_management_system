using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

public class StudentProfileViewModel
{
    public string StudentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Display(Name = "Age")]
    public int Age { get; set; }

    [Range(0.0, 300.0)]
    [Display(Name = "Height (cm)")]
    public decimal HeightCm { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    [MaxLength(20)]
    [Display(Name = "Mobile Number")]
    public string? MobileNumber { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}
