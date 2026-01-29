using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.Update;

public interface IStudentUpdateService
{
    Task<FieldUpdateResult> UpdateFullNameAsync(string userId, string value, CancellationToken cancellationToken = default);
    Task<FieldUpdateResult> UpdateDateOfBirthAsync(string userId, string value, CancellationToken cancellationToken = default);
    Task<FieldUpdateResult> UpdateHeightCmAsync(string userId, string value, CancellationToken cancellationToken = default);
    Task<FieldUpdateResult> UpdateGenderAsync(string userId, string value, CancellationToken cancellationToken = default);
    Task<FieldUpdateResult> UpdateMobileNumberAsync(string userId, string value, CancellationToken cancellationToken = default);
    Task<FieldUpdateResult> UpdateEmailAsync(string userId, string value, CancellationToken cancellationToken = default);

    Task<(bool Success, IReadOnlyList<string> Errors)> UpdateFromEditViewModelAsync(string userId, AdminStudentEditViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, IReadOnlyList<string> Errors)> UpdateFromProfileViewModelAsync(string userId, StudentProfileViewModel model, CancellationToken cancellationToken = default);
}
