using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using StudentManagementSystem.Configuration;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Student.Registration;
using StudentManagementSystem.Services.Student.Upload;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStudentRegistrationService _registrationService;
    private readonly IStudentFileUploadService _uploadService;
    private readonly IRegistrationDraftService _draftService;
    private readonly IOptions<TempUploadOptions> _tempUploadOptions;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, IStudentRegistrationService registrationService, IStudentFileUploadService uploadService, IRegistrationDraftService draftService, IOptions<TempUploadOptions> tempUploadOptions, IWebHostEnvironment env, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _registrationService = registrationService;
        _uploadService = uploadService;
        _draftService = draftService;
        _tempUploadOptions = tempUploadOptions;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register(CancellationToken cancellationToken)
    {
        _uploadService.CleanupExpiredDraftFolders();
        await _draftService.DeleteExpiredDraftsAsync(_tempUploadOptions.Value.ExpiryMinutes, cancellationToken);
        var draftId = await _draftService.CreateDraftAsync(cancellationToken);
        return View(new StudentRegistrationViewModel { DraftId = draftId.ToString() });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDraft(CancellationToken cancellationToken)
    {
        _uploadService.CleanupExpiredDraftFolders();
        await _draftService.DeleteExpiredDraftsAsync(_tempUploadOptions.Value.ExpiryMinutes, cancellationToken);
        var draftId = await _draftService.CreateDraftAsync(cancellationToken);
        return Json(new { draftId });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDraft(Guid draftId, string field, string? value, CancellationToken cancellationToken)
    {
        var ok = await _draftService.UpdateFieldAsync(draftId, field, value, cancellationToken);
        return Json(new { success = ok });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 110 * 1024 * 1024)]
    [RequestSizeLimit(110 * 1024 * 1024)]
    public async Task<IActionResult> UploadTempFile(string type, IFormFile file, Guid draftId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(type) || file == null || file.Length == 0 || draftId == Guid.Empty)
            return Json(new { success = false, error = "Invalid request." });

        var result = await _uploadService.SaveDraftFileAsync(type, file, draftId, cancellationToken);
        if (!result.Success)
            return Json(new { success = false, error = result.ErrorMessage });

        var field = string.Equals(type, "image", StringComparison.OrdinalIgnoreCase) ? "ProfileImagePath" : "ProfileVideoPath";
        await _draftService.UpdateFieldAsync(draftId, field, result.RelativePath, cancellationToken);

        return Json(new { success = true, path = result.RelativePath });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 110 * 1024 * 1024)] // 110 MB (100 MB video + 5 MB image + form)
    [RequestSizeLimit(110 * 1024 * 1024)]
    public async Task<IActionResult> Register(StudentRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _registrationService.RegisterAsync(model);

            if (!result.Success)
            {
                AddRegistrationErrorsToModelState(ModelState, result.Errors, model);
                return View(model);
            }

            TempData["SuccessMessage"] = result.TempDataMessage;
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", model.Email);
            var msg = _env.IsDevelopment()
                ? $"Registration failed: {ex.Message}"
                : "Registration failed. Please try again.";
            ModelState.AddModelError(string.Empty, msg);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>Adds registration errors to ModelState. Binds "Email already registered" to Email field so validation appears next to the input.</summary>
    private static void AddRegistrationErrorsToModelState(ModelStateDictionary modelState, IReadOnlyList<string> errors, StudentRegistrationViewModel model)
    {
        var isEmailError = errors.Count == 1 &&
            errors[0].Contains("Email", StringComparison.OrdinalIgnoreCase) &&
            errors[0].Contains("already", StringComparison.OrdinalIgnoreCase);
        var key = isEmailError ? nameof(model.Email) : string.Empty;
        foreach (var error in errors)
            modelState.AddModelError(key, error);
    }
}
