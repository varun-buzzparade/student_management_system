using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

public sealed class StudentQueryService : IStudentQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public StudentQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminStudentListViewModel> GetPagedStudentsAsync(StudentListQueryViewModel query, CancellationToken cancellationToken = default)
    {
        NormalizeQuery(query);

        // Restrict to users in Student role (admin list excludes admins)
        var studentRoleId = await _dbContext.Roles
            .Where(r => r.Name == Roles.Student)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IQueryable<ApplicationUser> q = _dbContext.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(studentRoleId))
        {
            q = from user in _dbContext.Users.AsNoTracking()
                join ur in _dbContext.UserRoles on user.Id equals ur.UserId
                where ur.RoleId == studentRoleId
                select user;
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
            q = q.Where(u => EF.Functions.ILike(u.FullName, $"%{query.Name}%"));
        if (query.Age.HasValue)
            q = q.Where(u => u.Age == query.Age.Value);
        if (query.Gender.HasValue)
            q = q.Where(u => u.Gender == query.Gender.Value);
        if (!string.IsNullOrWhiteSpace(query.MobileNumber))
            q = q.Where(u => u.MobileNumber != null && EF.Functions.ILike(u.MobileNumber, $"%{query.MobileNumber}%"));
        if (!string.IsNullOrWhiteSpace(query.Email))
            q = q.Where(u => u.Email != null && EF.Functions.ILike(u.Email, $"%{query.Email}%"));

        var total = await q.CountAsync(cancellationToken);
        var items = await q
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
            .ToListAsync(cancellationToken);

        return new AdminStudentListViewModel
        {
            Query = query,
            Results = new PagedResult<AdminStudentListItemViewModel>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = total
            }
        };
    }

    public async Task<ApplicationUser?> GetStudentByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    private static void NormalizeQuery(StudentListQueryViewModel query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize is < 5 or > 100) query.PageSize = 20;
    }
}
