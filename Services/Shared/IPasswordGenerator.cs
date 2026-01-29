namespace StudentManagementSystem.Services.Shared;

public interface IPasswordGenerator
{
    string Generate(int length = 12);
}
