using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.List;

public interface IStudentQueryService
{
    /// <summary>
    /// Returns paged, filtered student list items. Reusable across pages (admin list, exports, etc.).
    /// </summary>
    Task<PagedResult<AdminStudentListItemViewModel>> GetPagedStudentItemsAsync(StudentListQueryViewModel query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as GetPagedStudentItemsAsync but wrapped in AdminStudentListViewModel (Query + Results).
    /// </summary>
    Task<AdminStudentListViewModel> GetPagedStudentsAsync(StudentListQueryViewModel query, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetStudentByIdAsync(string id, CancellationToken cancellationToken = default);
}
