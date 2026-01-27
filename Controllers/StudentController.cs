using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

[Authorize(Roles = Roles.Student)]
public class StudentController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = new StudentProfileViewModel
        {
            StudentId = user.StudentId ?? string.Empty,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            Age = user.Age,
            HeightCm = user.HeightCm,
            Gender = user.Gender,
            MobileNumber = user.MobileNumber,
            Email = user.Email ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(StudentProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
                return View(model);
            }

            var emailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var userNameResult = await _userManager.SetUserNameAsync(user, model.Email);
            if (!userNameResult.Succeeded)
            {
                foreach (var error in userNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        // Calculate age from date of birth
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - model.DateOfBirth.Year;
        if (model.DateOfBirth > today.AddYears(-age)) age--;

        user.FullName = model.FullName;
        user.DateOfBirth = model.DateOfBirth;
        user.Age = age;
        user.HeightCm = model.HeightCm;
        user.Gender = model.Gender;
        user.MobileNumber = model.MobileNumber;
        user.PhoneNumber = model.MobileNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        ViewData["SuccessMessage"] = "Profile updated successfully.";
        model.StudentId = user.StudentId ?? string.Empty;
        return View(model);
    }
}
