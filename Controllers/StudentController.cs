using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

[Authorize(Roles = Roles.Student)]
public class StudentController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentViewModelMapper _mapper;
    private readonly IStudentUpdateService _updateService;

    public StudentController(
        UserManager<ApplicationUser> userManager,
        IStudentViewModelMapper mapper,
        IStudentUpdateService updateService)
    {
        _userManager = userManager;
        _mapper = mapper;
        _updateService = updateService;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        return View(_mapper.ToProfileViewModel(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(StudentProfileViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var (success, errors) = await _updateService.UpdateFromProfileViewModelAsync(user.Id, model, cancellationToken);
        if (!success)
        {
            foreach (var e in errors)
                ModelState.AddModelError(string.Empty, e);
            return View(model);
        }

        ViewData["SuccessMessage"] = "Profile updated successfully.";
        model.StudentId = user.StudentId ?? string.Empty;
        return View(model);
    }

    // AJAX endpoints used by Student/Profile view for per-field updates
    [HttpPost]
    public async Task<IActionResult> UpdateFullName(string value, CancellationToken cancellationToken = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await _updateService.UpdateFullNameAsync(userId, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDateOfBirth(string value, CancellationToken cancellationToken = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await _updateService.UpdateDateOfBirthAsync(userId, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message, age = r.Age });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateHeightCm(string value, CancellationToken cancellationToken = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await _updateService.UpdateHeightCmAsync(userId, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateGender(string value, CancellationToken cancellationToken = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await _updateService.UpdateGenderAsync(userId, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMobileNumber(string value, CancellationToken cancellationToken = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await _updateService.UpdateMobileNumberAsync(userId, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEmail(string value, CancellationToken cancellationToken = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await _updateService.UpdateEmailAsync(userId, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }
}
