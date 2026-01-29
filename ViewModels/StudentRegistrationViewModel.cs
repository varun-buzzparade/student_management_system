using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StudentManagementSystem.ViewModels;

public class StudentRegistrationViewModel : StudentFormBaseViewModel
{
    [Display(Name = "Profile Image")]
    public IFormFile? ProfileImage { get; set; }

    [Display(Name = "Profile Video")]
    public IFormFile? ProfileVideo { get; set; }
}
