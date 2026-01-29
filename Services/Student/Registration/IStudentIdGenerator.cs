namespace StudentManagementSystem.Services.Student.Registration;

public interface IStudentIdGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
