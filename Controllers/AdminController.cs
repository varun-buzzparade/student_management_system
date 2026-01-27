using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] StudentListQueryViewModel query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 5 or > 100 ? 20 : query.PageSize;

        var studentRoleId = await _dbContext.Roles
            .Where(r => r.Name == Roles.Student)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        IQueryable<ApplicationUser> studentsQuery = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(studentRoleId))
        {
            studentsQuery =
                from user in _dbContext.Users.AsNoTracking()
                join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                where userRole.RoleId == studentRoleId
                select user;
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            studentsQuery = studentsQuery.Where(u => EF.Functions.ILike(u.FullName, $"%{query.Name}%"));
        }

        if (query.Age.HasValue)
        {
            studentsQuery = studentsQuery.Where(u => u.Age == query.Age.Value);
        }

        if (query.Gender.HasValue)
        {
            studentsQuery = studentsQuery.Where(u => u.Gender == query.Gender.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.MobileNumber))
        {
            studentsQuery = studentsQuery.Where(u =>
                u.MobileNumber != null && EF.Functions.ILike(u.MobileNumber, $"%{query.MobileNumber}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            studentsQuery = studentsQuery.Where(u =>
                u.Email != null && EF.Functions.ILike(u.Email, $"%{query.Email}%"));
        }

        var totalCount = await studentsQuery.CountAsync();
        var items = await studentsQuery
            .OrderBy(u => u.FullName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new AdminStudentListItemViewModel
            {
                Id = u.Id,
                StudentId = u.StudentId ?? string.Empty,
                FullName = u.FullName,
                Age = u.Age,
                Gender = u.Gender,
                MobileNumber = u.MobileNumber,
                Email = u.Email ?? string.Empty
            })
            .ToListAsync();

        var model = new AdminStudentListViewModel
        {
            Query = query,
            Results = new PagedResult<AdminStudentListItemViewModel>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            }
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new AdminStudentEditViewModel
        {
            Id = user.Id,
            StudentId = user.StudentId ?? string.Empty,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            Age = user.Age,
            HeightCm = user.HeightCm,
            Gender = user.Gender,
            MobileNumber = user.MobileNumber,
            Email = user.Email ?? string.Empty
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new AdminStudentEditViewModel
        {
            Id = user.Id,
            StudentId = user.StudentId ?? string.Empty,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            Age = user.Age,
            HeightCm = user.HeightCm,
            Gender = user.Gender,
            MobileNumber = user.MobileNumber,
            Email = user.Email ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminStudentEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound();
        }

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
                return View(model);
            }

            var emailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var userNameResult = await _userManager.SetUserNameAsync(user, model.Email);
            if (!userNameResult.Succeeded)
            {
                foreach (var error in userNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        // Calculate age from date of birth
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - model.DateOfBirth.Year;
        if (model.DateOfBirth > today.AddYears(-age)) age--;

        user.FullName = model.FullName;
        user.DateOfBirth = model.DateOfBirth;
        user.Age = age;
        user.HeightCm = model.HeightCm;
        user.Gender = model.Gender;
        user.MobileNumber = model.MobileNumber;
        user.PhoneNumber = model.MobileNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["SuccessMessage"] = "Student updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }
}
