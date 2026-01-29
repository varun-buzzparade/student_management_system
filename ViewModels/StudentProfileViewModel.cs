using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.ViewModels;

public class StudentProfileViewModel : StudentFormBaseViewModel
{
    public string StudentId { get; set; } = string.Empty;

    [Display(Name = "Age")]
    public int Age { get; set; }
}
