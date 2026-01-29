using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Saves student profile images and videos under wwwroot/uploads, with size and format validation.
/// Image: max 5 MB, .jpeg/.jpg/.png. Video: max 100 MB, .mp4/.mov/.mkv/.avi/.wmv.
/// </summary>
public sealed class StudentFileUploadService : IStudentFileUploadService
{
    private const int ImageMaxBytes = 5 * 1024 * 1024;   // 5 MB
    private const int VideoMaxBytes = 100 * 1024 * 1024; // 100 MB

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpeg", ".jpg", ".png" };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".mov", ".mkv", ".avi", ".wmv" };

    private readonly IWebHostEnvironment _env;

    public StudentFileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<FileUploadResult> SaveImageAsync(IFormFile file, string studentId, CancellationToken cancellationToken = default)
    {
        var (ok, error) = ValidateFile(file, ImageMaxBytes, ImageExtensions, "Image", "JPEG, JPG, PNG", "5 MB");
        if (!ok)
            return new FileUploadResult(false, null, error);

        var ext = Path.GetExtension(file.FileName);
        var dir = Path.Combine(_env.WebRootPath, "uploads", "images", studentId);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, fileName);
        Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await file.CopyToAsync(stream, cancellationToken);

        var relativePath = Path.Combine("uploads", "images", studentId, fileName).Replace('\\', '/');
        return new FileUploadResult(true, relativePath, null);
    }

    public async Task<FileUploadResult> SaveVideoAsync(IFormFile file, string studentId, CancellationToken cancellationToken = default)
    {
        var (ok, error) = ValidateFile(file, VideoMaxBytes, VideoExtensions, "Video", "MP4, MOV, MKV, AVI, WMV", "100 MB");
        if (!ok)
            return new FileUploadResult(false, null, error);

        var ext = Path.GetExtension(file.FileName);
        var dir = Path.Combine(_env.WebRootPath, "uploads", "videos", studentId);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, fileName);
        Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await file.CopyToAsync(stream, cancellationToken);

        var relativePath = Path.Combine("uploads", "videos", studentId, fileName).Replace('\\', '/');
        return new FileUploadResult(true, relativePath, null);
    }

    private static (bool Ok, string? Error) ValidateFile(
        IFormFile file,
        int maxBytes,
        HashSet<string> allowedExtensions,
        string label,
        string allowedList,
        string maxSizeDisplay)
    {
        if (file == null || file.Length == 0)
            return (false, $"{label} file is empty or missing.");

        if (file.Length > maxBytes)
            return (false, $"{label} must be at most {maxSizeDisplay}.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            return (false, $"{label} must be one of: {allowedList}.");

        return (true, null);
    }
}
