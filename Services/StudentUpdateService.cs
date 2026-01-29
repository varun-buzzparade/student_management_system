using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return FieldUpdateResult.Fail("Student not found");

        user.FullName = value;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? FieldUpdateResult.Ok("Full Name updated") : FieldUpdateResult.Fail("Failed to update");
    }

    public async Task<FieldUpdateResult> UpdateDateOfBirthAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return FieldUpdateResult.Fail("Student not found");

        if (!DateOnly.TryParse(value, out var dob))
            return FieldUpdateResult.Fail("Invalid date format");

        var age = _ageCalculator.CalculateAge(dob);
        user.DateOfBirth = dob;
        user.Age = age;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? FieldUpdateResult.Ok("Date of Birth updated", age) : FieldUpdateResult.Fail("Failed to update");
    }

    public async Task<FieldUpdateResult> UpdateHeightCmAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return FieldUpdateResult.Fail("Student not found");

        if (!decimal.TryParse(value, out var height))
            return FieldUpdateResult.Fail("Invalid height value");

        user.HeightCm = height;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? FieldUpdateResult.Ok("Height updated") : FieldUpdateResult.Fail("Failed to update");
    }

    public async Task<FieldUpdateResult> UpdateGenderAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return FieldUpdateResult.Fail("Student not found");

        if (!Enum.TryParse<Gender>(value, out var gender))
            return FieldUpdateResult.Fail("Invalid gender value");

        user.Gender = gender;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? FieldUpdateResult.Ok("Gender updated") : FieldUpdateResult.Fail("Failed to update");
    }

    public async Task<FieldUpdateResult> UpdateMobileNumberAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return FieldUpdateResult.Fail("Student not found");

        user.MobileNumber = value;
        user.PhoneNumber = value;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? FieldUpdateResult.Ok("Mobile Number updated") : FieldUpdateResult.Fail("Failed to update");
    }

    public async Task<FieldUpdateResult> UpdateEmailAsync(string userId, string value, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return FieldUpdateResult.Fail("Student not found");

        if (string.Equals(user.Email, value, StringComparison.OrdinalIgnoreCase))
            return FieldUpdateResult.Ok("Email unchanged");

        var existing = await _userManager.FindByEmailAsync(value);
        if (existing != null && existing.Id != userId)
            return FieldUpdateResult.Fail("Email is already in use");

        var emailResult = await _userManager.SetEmailAsync(user, value);
        if (!emailResult.Succeeded)
            return FieldUpdateResult.Fail("Failed to update email");

        // Identity uses UserName for login; we keep it in sync with Email
        var userNameResult = await _userManager.SetUserNameAsync(user, value);
        if (!userNameResult.Succeeded)
            return FieldUpdateResult.Fail("Failed to update username");

        return FieldUpdateResult.Ok("Email updated");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateFromEditViewModelAsync(string userId, AdminStudentEditViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "Student not found" });

        var errors = new List<string>();

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != userId)
            {
                errors.Add("Email is already in use.");
                return (false, errors);
            }

            var er = await _userManager.SetEmailAsync(user, model.Email);
            if (!er.Succeeded) { errors.AddRange(er.Errors.Select(e => e.Description)); return (false, errors); }

            var ur = await _userManager.SetUserNameAsync(user, model.Email); // Identity login uses UserName
            if (!ur.Succeeded) { errors.AddRange(ur.Errors.Select(e => e.Description)); return (false, errors); }
        }

        var age = _ageCalculator.CalculateAge(model.DateOfBirth);
        user.FullName = model.FullName;
        user.DateOfBirth = model.DateOfBirth;
        user.Age = age;
        user.HeightCm = model.HeightCm;
        user.Gender = model.Gender;
        user.MobileNumber = model.MobileNumber;
        user.PhoneNumber = model.MobileNumber;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            errors.AddRange(update.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateFromProfileViewModelAsync(string userId, StudentProfileViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        var errors = new List<string>();

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != userId)
            {
                errors.Add("Email is already in use.");
                return (false, errors);
            }

            var er = await _userManager.SetEmailAsync(user, model.Email);
            if (!er.Succeeded) { errors.AddRange(er.Errors.Select(e => e.Description)); return (false, errors); }

            var ur = await _userManager.SetUserNameAsync(user, model.Email); // Identity login uses UserName
            if (!ur.Succeeded) { errors.AddRange(ur.Errors.Select(e => e.Description)); return (false, errors); }
        }

        var age = _ageCalculator.CalculateAge(model.DateOfBirth);
        user.FullName = model.FullName;
        user.DateOfBirth = model.DateOfBirth;
        user.Age = age;
        user.HeightCm = model.HeightCm;
        user.Gender = model.Gender;
        user.MobileNumber = model.MobileNumber;
        user.PhoneNumber = model.MobileNumber;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            errors.AddRange(update.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, Array.Empty<string>());
    }
}
