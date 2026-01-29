using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

public sealed class StudentRegistrationService : IStudentRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailSenderService _emailSender;
    private readonly IStudentIdGenerator _studentIdGenerator;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IAgeCalculator _ageCalculator;
    private readonly IStudentListCacheService _cache;

    public StudentRegistrationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailSenderService emailSender,
        IStudentIdGenerator studentIdGenerator,
        IPasswordGenerator passwordGenerator,
        IAgeCalculator ageCalculator,
        IStudentListCacheService cache)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _studentIdGenerator = studentIdGenerator;
        _passwordGenerator = passwordGenerator;
        _ageCalculator = ageCalculator;
        _cache = cache;
    }

    public async Task<RegistrationResult> RegisterAsync(StudentRegistrationViewModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing != null)
            return new RegistrationResult { Success = false, Errors = new[] { "Email is already registered." } };

        if (!await _roleManager.RoleExistsAsync(Roles.Student))
            await _roleManager.CreateAsync(new IdentityRole(Roles.Student));

        var studentId = await _studentIdGenerator.GenerateAsync(cancellationToken);
        var password = _passwordGenerator.Generate();
        var age = _ageCalculator.CalculateAge(model.DateOfBirth);

        var user = new ApplicationUser
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

        var create = await _userManager.CreateAsync(user, password);
        if (!create.Succeeded)
            return new RegistrationResult { Success = false, Errors = create.Errors.Select(e => e.Description).ToList() };

        await _userManager.AddToRoleAsync(user, Roles.Student);
        await _cache.InvalidateAsync(cancellationToken); // Admin list cache must reflect new student

        var emailSent = false;
        try
        {
            var body = $"""
                <p>Hello {user.FullName},</p>
                <p>Your student account has been created successfully.</p>
                <p><strong>Student ID:</strong> {studentId}</p>
                <p><strong>Login Email:</strong> {user.Email}</p>
                <p><strong>Temporary Password:</strong> {password}</p>
                <p>Please log in and update your profile as needed.</p>
                """;
            await _emailSender.SendEmailAsync(user.Email!, "Your Student Portal Credentials", body);
            emailSent = true;
        }
        catch
        {
            // Delivery failed; RegistrationResult.EmailSent = false, TempData shows credentials to save
        }

        return new RegistrationResult
        {
            Success = true,
            EmailSent = emailSent,
            StudentId = studentId,
            Email = user.Email,
            TempPassword = password
        };
    }
}
