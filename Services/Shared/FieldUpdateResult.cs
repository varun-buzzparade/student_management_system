namespace StudentManagementSystem.Services.Shared;

public sealed class FieldUpdateResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? Age { get; init; }

    public static FieldUpdateResult Ok(string message, int? age = null) =>
        new() { Success = true, Message = message, Age = age };

    public static FieldUpdateResult Fail(string message) =>
        new() { Success = false, Message = message };
}
