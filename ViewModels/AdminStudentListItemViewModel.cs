using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

public class AdminStudentListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? MobileNumber { get; set; }
    public string Email { get; set; } = string.Empty;
}
