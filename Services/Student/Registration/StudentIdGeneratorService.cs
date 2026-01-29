using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services.Student.Registration;

/// <summary>Generates unique StudentId values (STU + timestamp + random). Uses Guid suffix on collision.</summary>
public sealed class StudentIdGeneratorService : IStudentIdGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public StudentIdGeneratorService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"STU{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
            var exists = await _dbContext.Users.AnyAsync(u => u.StudentId == candidate, cancellationToken);
            if (!exists)
                return candidate;
        }

        // Collision fallback: use Guid suffix, truncate to 25 chars for unique index
        return $"STU{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..25];
    }
}
