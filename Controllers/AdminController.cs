using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.Services.Student.List;
using StudentManagementSystem.Services.Student.Mapping;
using StudentManagementSystem.Services.Student.Update;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IStudentListCacheService _listCache;
    private readonly IStudentQueryService _queryService;
    private readonly IStudentViewModelMapper _mapper;
    private readonly IStudentUpdateService _updateService;

    public AdminController(
        IStudentListCacheService listCache,
        IStudentQueryService queryService,
        IStudentViewModelMapper mapper,
        IStudentUpdateService updateService)
    {
        _listCache = listCache;
        _queryService = queryService;
        _mapper = mapper;
        _updateService = updateService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] StudentListQueryViewModel query, CancellationToken cancellationToken = default)
    {
        StudentListQueryNormalizer.Normalize(query);
        var model = await _listCache.GetOrAddAsync(
            query,
            (q, ct) => _queryService.GetPagedStudentsAsync(q, ct),
            cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
    {
        var user = await _queryService.GetStudentByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound();

        return View(_mapper.ToAdminEditViewModel(user));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
    {
        var user = await _queryService.GetStudentByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound();

        return View(_mapper.ToAdminEditViewModel(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminStudentEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, errors) = await _updateService.UpdateFromEditViewModelAsync(model.Id, model, cancellationToken);
        if (!success)
        {
            foreach (var e in errors)
                ModelState.AddModelError(string.Empty, e);
            return View(model);
        }

        TempData["SuccessMessage"] = "Student updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    // AJAX endpoints used by Admin/Edit view for per-field updates (no full-page POST)
    [HttpPost]
    public Task<IActionResult> UpdateFullName(string id, string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(id, value, _updateService.UpdateFullNameAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateDateOfBirth(string id, string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(id, value, _updateService.UpdateDateOfBirthAsync, cancellationToken, includeAge: true);

    [HttpPost]
    public Task<IActionResult> UpdateHeightCm(string id, string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(id, value, _updateService.UpdateHeightCmAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateGender(string id, string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(id, value, _updateService.UpdateGenderAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateMobileNumber(string id, string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(id, value, _updateService.UpdateMobileNumberAsync, cancellationToken, includeAge: false);

    [HttpPost]
    public Task<IActionResult> UpdateEmail(string id, string value, CancellationToken cancellationToken = default) =>
        UpdateFieldAsync(id, value, _updateService.UpdateEmailAsync, cancellationToken, includeAge: false);

    /// <summary>Shared logic for AJAX per-field updates: call update service, return JSON. Used by all Update* actions.</summary>
    private async Task<IActionResult> UpdateFieldAsync(
        string id,
        string value,
        Func<string, string, CancellationToken, Task<FieldUpdateResult>> update,
        CancellationToken cancellationToken,
        bool includeAge)
    {
        var r = await update(id, value, cancellationToken);
        return includeAge
            ? Json(new { success = r.Success, message = r.Message, age = r.Age })
            : Json(new { success = r.Success, message = r.Message });
    }
}
