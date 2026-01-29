using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.List;

public interface IStudentListCacheService
{
    Task<AdminStudentListViewModel> GetOrAddAsync(
        StudentListQueryViewModel query,
        Func<StudentListQueryViewModel, CancellationToken, Task<AdminStudentListViewModel>> factory,
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
