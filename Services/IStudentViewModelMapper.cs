using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

public interface IStudentViewModelMapper
{
    AdminStudentEditViewModel ToAdminEditViewModel(ApplicationUser user);
    StudentProfileViewModel ToProfileViewModel(ApplicationUser user);
}
