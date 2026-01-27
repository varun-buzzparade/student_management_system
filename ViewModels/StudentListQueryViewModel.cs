using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

public class StudentListQueryViewModel
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public Gender? Gender { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
