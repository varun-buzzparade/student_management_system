using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.Services.Student.Mapping;
using StudentManagementSystem.Services.Student.Update;
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
    public Task<IActionResult> UpdateFullName(string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(value, _updateService.UpdateFullNameAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateDateOfBirth(string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(value, _updateService.UpdateDateOfBirthAsync, cancellationToken, includeAge: true);

    [HttpPost]
    public Task<IActionResult> UpdateHeightCm(string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(value, _updateService.UpdateHeightCmAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateGender(string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(value, _updateService.UpdateGenderAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateMobileNumber(string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(value, _updateService.UpdateMobileNumberAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateEmail(string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(value, _updateService.UpdateEmailAsync, cancellationToken, includeAge: false);

    /// <summary>Shared logic for AJAX per-field updates: resolve userId, call update service, return JSON. Used by all Update* actions.</summary>
    private async Task<IActionResult> UpdateFieldAsync(
        string value,
        Func<string, string, CancellationToken, Task<FieldUpdateResult>> update,
        CancellationToken cancellationToken,
        bool includeAge)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "User not found" });

        var r = await update(userId, value, cancellationToken);
        return includeAge
            ? Json(new { success = r.Success, message = r.Message, age = r.Age })
            : Json(new { success = r.Success, message = r.Message });
    }
}
