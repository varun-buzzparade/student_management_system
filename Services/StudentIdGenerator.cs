using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;

namespace StudentManagementSystem.Services;

public static class StudentIdGenerator
{
    public static async Task<string> GenerateAsync(ApplicationDbContext dbContext)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"STU{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
            var exists = await dbContext.Users.AnyAsync(u => u.StudentId == candidate);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"STU{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..25];
    }
}
