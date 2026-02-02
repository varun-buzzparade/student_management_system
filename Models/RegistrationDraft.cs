using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Services.Student.Upload;

namespace StudentManagementSystem.Models;

/// <summary>
/// Stores partial registration data as user fills the form. Deleted on submit or after expiry.
/// </summary>
public class RegistrationDraft : IDraftWithFilePaths
{
    public Guid Id { get; set; }

    [MaxLength(150)]
    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Range(0.0, 300.0)]
    public decimal? HeightCm { get; set; }

    public Gender? Gender { get; set; }

    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    /// <summary>Path as stored in DB (e.g. draft/{draftId}/images/file.jpg). Same format as final user path.</summary>
    [MaxLength(500)]
    public string? ProfileImagePath { get; set; }

    [MaxLength(500)]
    public string? ProfileVideoPath { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
