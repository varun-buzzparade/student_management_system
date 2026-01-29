namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Handles validation and storage of student profile image and video uploads.
/// Image: max 5 MB, JPEG/JPG/PNG. Video: max 100 MB, MP4/MOV/MKV/AVI/WMV.
/// </summary>
public interface IStudentFileUploadService
{
    /// <summary>
    /// Validates and saves a profile image. Returns relative path (e.g. uploads/images/{studentId}/file.jpg) or an error.
    /// </summary>
    Task<FileUploadResult> SaveImageAsync(IFormFile file, string studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and saves a profile video. Returns relative path (e.g. uploads/videos/{studentId}/file.mp4) or an error.
    /// </summary>
    Task<FileUploadResult> SaveVideoAsync(IFormFile file, string studentId, CancellationToken cancellationToken = default);
}

/// <summary>Result of a single file upload attempt.</summary>
public sealed record FileUploadResult(bool Success, string? RelativePath, string? ErrorMessage);
