using System.IO;
using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Email;
using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.Services.Student.List;
using StudentManagementSystem.Services.Student.Upload;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.Registration;

/// <summary>Orchestrates student registration: create user, assign role, invalidate list cache, send welcome email.</summary>
public sealed class StudentRegistrationService : IStudentRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailSenderService _emailSender;
    private readonly IStudentIdGenerator _studentIdGenerator;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IAgeCalculator _ageCalculator;
    private readonly IStudentListCacheService _cache;
    private readonly IStudentFileUploadService _uploadService;

    public StudentRegistrationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailSenderService emailSender,
        IStudentIdGenerator studentIdGenerator,
        IPasswordGenerator passwordGenerator,
        IAgeCalculator ageCalculator,
        IStudentListCacheService cache,
        IStudentFileUploadService uploadService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _studentIdGenerator = studentIdGenerator;
        _passwordGenerator = passwordGenerator;
        _ageCalculator = ageCalculator;
        _cache = cache;
        _uploadService = uploadService;
    }

    public async Task<RegistrationResult> RegisterAsync(StudentRegistrationViewModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing != null)
            return new RegistrationResult { Success = false, Errors = new[] { "Email is already registered." } };

        await EnsureStudentRoleExistsAsync();

        var studentId = await _studentIdGenerator.GenerateAsync(cancellationToken);
        var password = _passwordGenerator.Generate();
        var user = BuildNewStudentFromModel(model, studentId);

        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            var img = await _uploadService.SaveImageAsync(model.ProfileImage, studentId, cancellationToken);
            if (!img.Success)
                return new RegistrationResult { Success = false, Errors = new[] { img.ErrorMessage! } };
            user.ProfileImagePath = img.RelativePath;
        }

        if (model.ProfileVideo != null && model.ProfileVideo.Length > 0)
        {
            var vid = await _uploadService.SaveVideoAsync(model.ProfileVideo, studentId, cancellationToken);
            if (!vid.Success)
                return new RegistrationResult { Success = false, Errors = new[] { vid.ErrorMessage! } };
            user.ProfileVideoPath = vid.RelativePath;
        }

        var create = await _userManager.CreateAsync(user, password);
        if (!create.Succeeded)
            return new RegistrationResult { Success = false, Errors = create.Errors.Select(e => e.Description).ToList() };

        await _userManager.AddToRoleAsync(user, Roles.Student);
        await _cache.InvalidateAsync(cancellationToken); // Admin list cache must reflect new student

        var emailSent = await TrySendWelcomeEmailAsync(user, studentId, password, cancellationToken);

        return new RegistrationResult
        {
            Success = true,
            EmailSent = emailSent,
            StudentId = studentId,
            Email = user.Email,
            TempPassword = password
        };
    }

    private async Task EnsureStudentRoleExistsAsync()
    {
        if (!await _roleManager.RoleExistsAsync(Roles.Student))
            await _roleManager.CreateAsync(new IdentityRole(Roles.Student));
    }

    private ApplicationUser BuildNewStudentFromModel(StudentRegistrationViewModel model, string studentId)
    {
        var age = _ageCalculator.CalculateAge(model.DateOfBirth);
        return new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            DateOfBirth = model.DateOfBirth,
            Age = age,
            HeightCm = model.HeightCm,
            Gender = model.Gender,
            MobileNumber = model.MobileNumber,
            PhoneNumber = model.MobileNumber,
            StudentId = studentId
        };
    }

    private async Task<bool> TrySendWelcomeEmailAsync(ApplicationUser user, string studentId, string password, CancellationToken cancellationToken)
    {
        try
        {
            var body = await GetWelcomeEmailBodyAsync(user.FullName, user.Email!, studentId, password, cancellationToken);
            await _emailSender.SendEmailAsync(user.Email!, "Your Student Portal Credentials", body);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> GetWelcomeEmailBodyAsync(string fullName, string email, string studentId, string password, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "StudentWelcomeEmail.txt");
        var template = await File.ReadAllTextAsync(path, cancellationToken);
        return template
            .Replace("{{FullName}}", fullName)
            .Replace("{{StudentId}}", studentId)
            .Replace("{{Email}}", email)
            .Replace("{{Password}}", password);
    }
}
