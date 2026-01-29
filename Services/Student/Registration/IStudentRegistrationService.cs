using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.Registration;

public interface IStudentRegistrationService
{
    Task<RegistrationResult> RegisterAsync(StudentRegistrationViewModel model, CancellationToken cancellationToken = default);
}
