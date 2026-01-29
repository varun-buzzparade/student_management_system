using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.Mapping;

public interface IStudentViewModelMapper
{
    AdminStudentEditViewModel ToAdminEditViewModel(ApplicationUser user);
    StudentProfileViewModel ToProfileViewModel(ApplicationUser user);
}
