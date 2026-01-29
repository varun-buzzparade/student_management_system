using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

public interface IStudentQueryService
{
    Task<AdminStudentListViewModel> GetPagedStudentsAsync(StudentListQueryViewModel query, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetStudentByIdAsync(string id, CancellationToken cancellationToken = default);
}
