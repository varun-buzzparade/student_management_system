namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Minimal contract for draft file move. Decouples upload service from RegistrationDraft entity (DIP).
/// </summary>
public interface IDraftWithFilePaths
{
    Guid Id { get; }
    string? ProfileImagePath { get; }
    string? ProfileVideoPath { get; }
}

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

    /// <summary>
    /// Saves a draft file (pre-submit). Path stored in draft and reflected in DB. Uses system temp to avoid OneDrive locks.
    /// </summary>
    Task<DraftUploadResult> SaveDraftFileAsync(string type, IFormFile file, Guid draftId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves draft files to student folder and returns final paths. Deletes draft files after move.
    /// </summary>
    Task<(string? ImagePath, string? VideoPath)> MoveDraftFilesToStudentAsync(IDraftWithFilePaths draft, string studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes draft files from disk (used when draft expires or is abandoned).
    /// </summary>
    void DeleteDraftFiles(Guid draftId);

    /// <summary>
    /// Deletes expired draft folders (session timeout cleanup).
    /// </summary>
    void CleanupExpiredDraftFolders();
}

/// <summary>Result of a draft file upload (pre-submit). Path is stored in draft.</summary>
public sealed record DraftUploadResult(bool Success, string? RelativePath, string? ErrorMessage);

/// <summary>Result of a single file upload attempt.</summary>
public sealed record FileUploadResult(bool Success, string? RelativePath, string? ErrorMessage);
