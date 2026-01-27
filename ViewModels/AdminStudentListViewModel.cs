namespace StudentManagementSystem.ViewModels;

public class AdminStudentListViewModel
{
    public StudentListQueryViewModel Query { get; set; } = new();
    public PagedResult<AdminStudentListItemViewModel> Results { get; set; } = new();
}
