namespace StudentManagementSystem.Services;

public sealed class RegistrationResult
{
    public bool Success { get; init; }
    public bool EmailSent { get; init; }
    public string? StudentId { get; init; }
    public string? Email { get; init; }
    public string? TempPassword { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public string TempDataMessage =>
        EmailSent
            ? "Registration successful. Credentials have been emailed."
            : $"Registration successful! Please save your credentials:\n\nStudent ID: {StudentId}\nEmail: {Email}\nPassword: {TempPassword}\n\n(Email delivery failed - please save these credentials now!)";
}
