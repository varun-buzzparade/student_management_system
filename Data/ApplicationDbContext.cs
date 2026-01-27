using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.HeightCm).HasPrecision(5, 2);
            entity.Property(u => u.Gender).HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(u => u.StudentId).IsUnique();
            entity.HasIndex(u => u.FullName);
            entity.HasIndex(u => u.Age);
            entity.HasIndex(u => u.Gender);
            entity.HasIndex(u => u.MobileNumber);
        });
    }
}
