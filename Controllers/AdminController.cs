using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
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
        NormalizeListQuery(query);
        var model = await _listCache.GetOrAddAsync(
            query,
            (q, ct) => _queryService.GetPagedStudentsAsync(q, ct),
            cancellationToken);
        return View(model);
    }

    private static void NormalizeListQuery(StudentListQueryViewModel q)
    {
        if (q.Page < 1) q.Page = 1;
        if (q.PageSize is < 5 or > 100) q.PageSize = 20;
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
    public async Task<IActionResult> UpdateFullName(string id, string value, CancellationToken cancellationToken = default)
    {
        var r = await _updateService.UpdateFullNameAsync(id, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDateOfBirth(string id, string value, CancellationToken cancellationToken = default)
    {
        var r = await _updateService.UpdateDateOfBirthAsync(id, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message, age = r.Age });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateHeightCm(string id, string value, CancellationToken cancellationToken = default)
    {
        var r = await _updateService.UpdateHeightCmAsync(id, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateGender(string id, string value, CancellationToken cancellationToken = default)
    {
        var r = await _updateService.UpdateGenderAsync(id, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMobileNumber(string id, string value, CancellationToken cancellationToken = default)
    {
        var r = await _updateService.UpdateMobileNumberAsync(id, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEmail(string id, string value, CancellationToken cancellationToken = default)
    {
        var r = await _updateService.UpdateEmailAsync(id, value, cancellationToken);
        return Json(new { success = r.Success, message = r.Message });
    }
}
