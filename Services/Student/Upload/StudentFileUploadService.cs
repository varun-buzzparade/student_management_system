using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StudentManagementSystem.Configuration;
using StudentManagementSystem.Models;

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
    private readonly int _draftExpiryMinutes;

    public StudentFileUploadService(IWebHostEnvironment env, IOptions<TempUploadOptions> options)
    {
        _env = env;
        _draftExpiryMinutes = options.Value.ExpiryMinutes;
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

    /// <summary>Saves directly to uploads/images/{draftId}/ or uploads/videos/{draftId}/. On submit, folder is renamed to {studentId}. On timeout, folder is deleted.</summary>
    public async Task<DraftUploadResult> SaveDraftFileAsync(string type, IFormFile file, Guid draftId, CancellationToken cancellationToken = default)
    {
        var isImage = string.Equals(type, "image", StringComparison.OrdinalIgnoreCase);
        var (maxBytes, extensions, label, allowedList, maxSizeDisplay) = isImage
            ? (ImageMaxBytes, ImageExtensions, "Image", "JPEG, JPG, PNG", "5 MB")
            : (VideoMaxBytes, VideoExtensions, "Video", "MP4, MOV, MKV, AVI, WMV", "100 MB");

        var (ok, error) = ValidateFile(file, maxBytes, extensions, label, allowedList, maxSizeDisplay);
        if (!ok)
            return new DraftUploadResult(false, null, error);

        var ext = Path.GetExtension(file.FileName);
        var subdir = isImage ? "images" : "videos";
        var draftDir = Path.Combine(_env.WebRootPath, "uploads", subdir, draftId.ToString("N"));
        Directory.CreateDirectory(draftDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(draftDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await file.CopyToAsync(stream, cancellationToken);

        var relativePath = Path.Combine("uploads", subdir, draftId.ToString("N"), fileName).Replace('\\', '/');
        return new DraftUploadResult(true, relativePath, null);
    }

    /// <summary>Renames draft folders from {draftId} to {studentId}. Files already in uploads/images/ and uploads/videos/.</summary>
    public Task<(string? ImagePath, string? VideoPath)> MoveDraftFilesToStudentAsync(RegistrationDraft draft, string studentId, CancellationToken cancellationToken = default)
    {
        string? imagePath = null;
        string? videoPath = null;
        var did = draft.Id.ToString("N");

        var imgDraftDir = Path.Combine(_env.WebRootPath, "uploads", "images", did);
        var imgStudentDir = Path.Combine(_env.WebRootPath, "uploads", "images", studentId);
        if (Directory.Exists(imgDraftDir))
        {
            if (Directory.Exists(imgStudentDir)) Directory.Delete(imgStudentDir, true);
            Directory.Move(imgDraftDir, imgStudentDir);
            var fileName = Path.GetFileName(draft.ProfileImagePath ?? "");
            if (!string.IsNullOrEmpty(fileName))
                imagePath = Path.Combine("uploads", "images", studentId, fileName).Replace('\\', '/');
        }

        var vidDraftDir = Path.Combine(_env.WebRootPath, "uploads", "videos", did);
        var vidStudentDir = Path.Combine(_env.WebRootPath, "uploads", "videos", studentId);
        if (Directory.Exists(vidDraftDir))
        {
            if (Directory.Exists(vidStudentDir)) Directory.Delete(vidStudentDir, true);
            Directory.Move(vidDraftDir, vidStudentDir);
            var fileName = Path.GetFileName(draft.ProfileVideoPath ?? "");
            if (!string.IsNullOrEmpty(fileName))
                videoPath = Path.Combine("uploads", "videos", studentId, fileName).Replace('\\', '/');
        }

        return Task.FromResult((imagePath, videoPath));
    }

    public void DeleteDraftFiles(Guid draftId)
    {
        var did = draftId.ToString("N");
        foreach (var subdir in new[] { "images", "videos" })
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads", subdir, did);
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch { }
        }
    }

    public void CleanupExpiredDraftFolders()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-_draftExpiryMinutes);
        foreach (var subdir in new[] { "images", "videos" })
        {
            var baseDir = Path.Combine(_env.WebRootPath, "uploads", subdir);
            if (!Directory.Exists(baseDir)) continue;
            foreach (var dir in Directory.GetDirectories(baseDir))
            {
                try
                {
                    var di = new DirectoryInfo(dir);
                    if (Guid.TryParseExact(di.Name, "N", out _) && di.CreationTimeUtc < cutoff)
                        di.Delete(recursive: true);
                }
                catch { }
            }
        }
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
