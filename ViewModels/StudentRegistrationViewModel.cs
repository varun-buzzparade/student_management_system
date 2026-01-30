using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StudentManagementSystem.ViewModels;

public class StudentRegistrationViewModel : StudentFormBaseViewModel
{
    [Display(Name = "Profile Image")]
    public IFormFile? ProfileImage { get; set; }

    [Display(Name = "Profile Video")]
    public IFormFile? ProfileVideo { get; set; }

    /// <summary>Draft ID for auto-saved partial registration; deleted on submit or expiry.</summary>
    [HiddenInput]
    public string? DraftId { get; set; }
}
