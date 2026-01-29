using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStudentRegistrationService _registrationService;

    public AccountController(SignInManager<ApplicationUser> signInManager, IStudentRegistrationService registrationService)
    {
        _signInManager = signInManager;
        _registrationService = registrationService;
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
    public IActionResult Register()
    {
        return View(new StudentRegistrationViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(StudentRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _registrationService.RegisterAsync(model);

        if (!result.Success)
        {
            // Bind "Email is already registered" to Email field so validation appears next to the input
            var isEmailError = result.Errors.Count == 1 &&
                result.Errors[0].Contains("Email", StringComparison.OrdinalIgnoreCase) &&
                result.Errors[0].Contains("already", StringComparison.OrdinalIgnoreCase);
            foreach (var error in result.Errors)
                ModelState.AddModelError(isEmailError ? nameof(model.Email) : string.Empty, error);
            return View(model);
        }

        TempData["SuccessMessage"] = result.TempDataMessage;
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
