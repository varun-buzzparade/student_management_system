namespace StudentManagementSystem.Services;

public sealed class AgeCalculator : IAgeCalculator
{
    public int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
            age--;
        return age;
    }
}
