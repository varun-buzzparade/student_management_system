namespace StudentManagementSystem.Services;

public interface IPasswordGenerator
{
    string Generate(int length = 12);
}
