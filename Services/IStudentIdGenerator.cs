namespace StudentManagementSystem.Services;

public interface IStudentIdGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
