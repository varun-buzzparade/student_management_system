using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.Mapping;

/// <summary>
/// Maps ApplicationUser to view models for Admin (edit) and Student (profile) views.
/// </summary>
public sealed class StudentViewModelMapper : IStudentViewModelMapper
{
    private static readonly DateOnly DefaultDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

    public AdminStudentEditViewModel ToAdminEditViewModel(ApplicationUser user)
    {
        return new AdminStudentEditViewModel
        {
            Id = user.Id,
            StudentId = user.StudentId ?? string.Empty,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? DefaultDob,
            Age = user.Age,
            HeightCm = user.HeightCm,
            Gender = user.Gender,
            MobileNumber = user.MobileNumber,
            Email = user.Email ?? string.Empty
        };
    }

    public StudentProfileViewModel ToProfileViewModel(ApplicationUser user)
    {
        return new StudentProfileViewModel
        {
            StudentId = user.StudentId ?? string.Empty,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? DefaultDob,
            Age = user.Age,
            HeightCm = user.HeightCm,
            Gender = user.Gender,
            MobileNumber = user.MobileNumber,
            Email = user.Email ?? string.Empty
        };
    }
}
