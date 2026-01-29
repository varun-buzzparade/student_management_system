using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

public interface IStudentRegistrationService
{
    Task<RegistrationResult> RegisterAsync(StudentRegistrationViewModel model, CancellationToken cancellationToken = default);
}
