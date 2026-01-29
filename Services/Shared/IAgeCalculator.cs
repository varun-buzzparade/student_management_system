namespace StudentManagementSystem.Services.Shared;

public interface IAgeCalculator
{
    int CalculateAge(DateOnly dateOfBirth);
}
