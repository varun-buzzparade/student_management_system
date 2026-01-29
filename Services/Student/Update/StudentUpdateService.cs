using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services.Student.Update;

public sealed class StudentUpdateService : IStudentUpdateService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAgeCalculator _ageCalculator;

    public StudentUpdateService(UserManager<ApplicationUser> userManager, IAgeCalculator ageCalculator)
    {
        _userManager = userManager;
        _ageCalculator = ageCalculator;
    }

    public async Task<FieldUpdateResult> UpdateFullNameAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null) return fail;

        user!.FullName = value;
        return await SaveAndOkAsync(user, "Full Name updated");
    }

    public async Task<FieldUpdateResult> UpdateDateOfBirthAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null) return fail;

        if (!DateOnly.TryParse(value, out var dob))
            return FieldUpdateResult.Fail("Invalid date format");

        var age = _ageCalculator.CalculateAge(dob);
        user!.DateOfBirth = dob;
        user.Age = age;
        return await SaveAndOkAsync(user, "Date of Birth updated", age);
    }

    public async Task<FieldUpdateResult> UpdateHeightCmAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null) return fail;

        if (!decimal.TryParse(value, out var height))
            return FieldUpdateResult.Fail("Invalid height value");

        user!.HeightCm = height;
        return await SaveAndOkAsync(user, "Height updated");
    }

    public async Task<FieldUpdateResult> UpdateGenderAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null) return fail;

        if (!Enum.TryParse<Gender>(value, out var gender))
            return FieldUpdateResult.Fail("Invalid gender value");

        user!.Gender = gender;
        return await SaveAndOkAsync(user, "Gender updated");
    }

    public async Task<FieldUpdateResult> UpdateMobileNumberAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null) return fail;

        user!.MobileNumber = value;
        user.PhoneNumber = value;
        return await SaveAndOkAsync(user, "Mobile Number updated");
    }

    public async Task<FieldUpdateResult> UpdateEmailAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null) return fail;

        var u = user!;
        if (string.Equals(u.Email, value, StringComparison.OrdinalIgnoreCase))
            return FieldUpdateResult.Ok("Email unchanged");

        var existing = await _userManager.FindByEmailAsync(value);
        if (existing != null && existing.Id != userId)
            return FieldUpdateResult.Fail("Email is already in use");

        var emailResult = await _userManager.SetEmailAsync(u, value);
        if (!emailResult.Succeeded)
            return FieldUpdateResult.Fail("Failed to update email");

        // Identity uses UserName for login; keep in sync with Email
        var userNameResult = await _userManager.SetUserNameAsync(u, value);
        if (!userNameResult.Succeeded)
            return FieldUpdateResult.Fail("Failed to update username");

        return FieldUpdateResult.Ok("Email updated");
    }

    /// <summary>Returns (user, null) when found, or (null, fail) when not.</summary>
    private async Task<(ApplicationUser? User, FieldUpdateResult? Fail)> TryGetUserAsync(string userId, CancellationToken cancellationToken, string notFoundMessage = "Student not found")
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? (null, FieldUpdateResult.Fail(notFoundMessage)) : (user, null);
    }

    /// <summary>Persists user via UserManager.UpdateAsync; returns Ok or Fail.</summary>
    private async Task<FieldUpdateResult> SaveAndOkAsync(ApplicationUser user, string message, int? age = null)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? FieldUpdateResult.Ok(message, age) : FieldUpdateResult.Fail("Failed to update");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateFromEditViewModelAsync(string userId, AdminStudentEditViewModel model, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken);
        if (fail != null)
            return (false, new[] { fail.Message });

        var input = new ProfileUpdateInput(model.FullName, model.DateOfBirth, model.Email, model.HeightCm, model.Gender, model.MobileNumber);
        return await ApplyProfileUpdateAsync(user!, userId, input, cancellationToken);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateFromProfileViewModelAsync(string userId, StudentProfileViewModel model, CancellationToken cancellationToken = default)
    {
        var (user, fail) = await TryGetUserAsync(userId, cancellationToken, "User not found");
        if (fail != null)
            return (false, new[] { fail.Message });

        var input = new ProfileUpdateInput(model.FullName, model.DateOfBirth, model.Email, model.HeightCm, model.Gender, model.MobileNumber);
        return await ApplyProfileUpdateAsync(user!, userId, input, cancellationToken);
    }

    /// <summary>
    /// Shared email-update and profile-apply logic for both admin edit and student profile form submissions.
    /// </summary>
    private async Task<(bool Success, IReadOnlyList<string> Errors)> ApplyProfileUpdateAsync(ApplicationUser user, string userId, ProfileUpdateInput input, CancellationToken cancellationToken)
    {
        var (emailOk, emailErrors) = await TryUpdateEmailIfChangedAsync(user, userId, input.Email, cancellationToken);
        if (!emailOk)
            return (false, emailErrors);

        ApplyProfileFieldsToUser(user, input);

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            var errors = update.Errors.Select(e => e.Description).ToList();
            return (false, errors);
        }

        return (true, Array.Empty<string>());
    }

    /// <summary>Updates user Email and UserName when newEmail differs. Returns (false, errors) on duplicate or Identity failure.</summary>
    private async Task<(bool Success, List<string> Errors)> TryUpdateEmailIfChangedAsync(ApplicationUser user, string userId, string newEmail, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            return (true, errors);

        var existing = await _userManager.FindByEmailAsync(newEmail);
        if (existing != null && existing.Id != userId)
        {
            errors.Add("Email is already in use.");
            return (false, errors);
        }

        var er = await _userManager.SetEmailAsync(user, newEmail);
        if (!er.Succeeded)
        {
            errors.AddRange(er.Errors.Select(e => e.Description));
            return (false, errors);
        }

        var ur = await _userManager.SetUserNameAsync(user, newEmail);
        if (!ur.Succeeded)
        {
            errors.AddRange(ur.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, errors);
    }

    /// <summary>Sets FullName, DateOfBirth, Age, HeightCm, Gender, MobileNumber, PhoneNumber from input.</summary>
    private void ApplyProfileFieldsToUser(ApplicationUser user, ProfileUpdateInput input)
    {
        var age = _ageCalculator.CalculateAge(input.DateOfBirth);
        user.FullName = input.FullName;
        user.DateOfBirth = input.DateOfBirth;
        user.Age = age;
        user.HeightCm = input.HeightCm;
        user.Gender = input.Gender;
        user.MobileNumber = input.MobileNumber;
        user.PhoneNumber = input.MobileNumber;
    }

    private sealed record ProfileUpdateInput(string FullName, DateOnly DateOfBirth, string Email, decimal HeightCm, Gender Gender, string? MobileNumber);
}
