namespace StudentManagementSystem.Services;

public interface IAgeCalculator
{
    int CalculateAge(DateOnly dateOfBirth);
}
