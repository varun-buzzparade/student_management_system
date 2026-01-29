using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.ViewModels;

public class AdminStudentEditViewModel : AdminStudentBaseViewModel
{
    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Range(0.0, 300.0)]
    [Display(Name = "Height (cm)")]
    public decimal HeightCm { get; set; }
}
