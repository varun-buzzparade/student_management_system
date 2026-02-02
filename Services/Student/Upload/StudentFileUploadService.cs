using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StudentManagementSystem.Configuration;

namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Saves student profile images and videos under wwwroot/uploads, with size and format validation.
/// Uses UploadConstants for limits and extensions (DRY).
/// </summary>
public sealed class StudentFileUploadService : IStudentFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly int _draftExpiryMinutes;
    private readonly IBackgroundCompressionService _compression;

    public StudentFileUploadService(IWebHostEnvironment env, IOptions<TempUploadOptions> options, IBackgroundCompressionService compression)
    {
        _env = env;
        _draftExpiryMinutes = options.Value.ExpiryMinutes;
        _compression = compression;
    }

    public async Task<FileUploadResult> SaveImageAsync(IFormFile file, string studentId, CancellationToken cancellationToken = default)
    {
        var (ok, error) = ValidateFile(file, UploadConstants.ImageMaxBytes, UploadConstants.ImageExtensions, "Image", "JPEG, JPG, PNG", "5 MB");
        if (!ok)
            return new FileUploadResult(false, null, error);

        var ext = Path.GetExtension(file.FileName);
        var dir = Path.Combine(_env.WebRootPath, "uploads", UploadConstants.ImagesSubdir, studentId);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, fileName);
        Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await file.CopyToAsync(stream, cancellationToken);

        _compression.QueueForCompression(fullPath, UploadConstants.TypeImage);

        var relativePath = Path.Combine("uploads", UploadConstants.ImagesSubdir, studentId, fileName).Replace('\\', '/');
        return new FileUploadResult(true, relativePath, null);
    }

    public async Task<FileUploadResult> SaveVideoAsync(IFormFile file, string studentId, CancellationToken cancellationToken = default)
    {
        var (ok, error) = ValidateFile(file, UploadConstants.VideoMaxBytes, UploadConstants.VideoExtensions, "Video", "MP4, MOV, MKV, AVI, WMV", "100 MB");
        if (!ok)
            return new FileUploadResult(false, null, error);

        var ext = Path.GetExtension(file.FileName);
        var dir = Path.Combine(_env.WebRootPath, "uploads", UploadConstants.VideosSubdir, studentId);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, fileName);
        Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await file.CopyToAsync(stream, cancellationToken);

        _compression.QueueForCompression(fullPath, UploadConstants.TypeVideo);

        var relativePath = Path.Combine("uploads", UploadConstants.VideosSubdir, studentId, fileName).Replace('\\', '/');
        return new FileUploadResult(true, relativePath, null);
    }

    /// <summary>Saves directly to uploads/images/{draftId}/ or uploads/videos/{draftId}/. On submit, folder is renamed to {studentId}. On timeout, folder is deleted.</summary>
    public async Task<DraftUploadResult> SaveDraftFileAsync(string type, IFormFile file, Guid draftId, CancellationToken cancellationToken = default)
    {
        var isImage = string.Equals(type, UploadConstants.TypeImage, StringComparison.OrdinalIgnoreCase);
        var (maxBytes, extensions, label, allowedList, maxSizeDisplay) = isImage
            ? (UploadConstants.ImageMaxBytes, UploadConstants.ImageExtensions, "Image", "JPEG, JPG, PNG", "5 MB")
            : (UploadConstants.VideoMaxBytes, UploadConstants.VideoExtensions, "Video", "MP4, MOV, MKV, AVI, WMV", "100 MB");

        var (ok, error) = ValidateFile(file, maxBytes, extensions, label, allowedList, maxSizeDisplay);
        if (!ok)
            return new DraftUploadResult(false, null, error);

        var ext = Path.GetExtension(file.FileName);
        var subdir = isImage ? UploadConstants.ImagesSubdir : UploadConstants.VideosSubdir;
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
    public Task<(string? ImagePath, string? VideoPath)> MoveDraftFilesToStudentAsync(IDraftWithFilePaths draft, string studentId, CancellationToken cancellationToken = default)
    {
        var did = draft.Id.ToString("N");
        var imagePath = MoveDraftMediaAndQueue(did, studentId, UploadConstants.ImagesSubdir, draft.ProfileImagePath, UploadConstants.TypeImage);
        var videoPath = MoveDraftMediaAndQueue(did, studentId, UploadConstants.VideosSubdir, draft.ProfileVideoPath, UploadConstants.TypeVideo);
        return Task.FromResult((imagePath, videoPath));
    }

    private string? MoveDraftMediaAndQueue(string draftId, string studentId, string subdir, string? draftRelativePath, string compressionType)
    {
        var draftDir = Path.Combine(_env.WebRootPath, "uploads", subdir, draftId);
        var studentDir = Path.Combine(_env.WebRootPath, "uploads", subdir, studentId);
        if (!Directory.Exists(draftDir)) return null;

        if (Directory.Exists(studentDir)) Directory.Delete(studentDir, true);
        Directory.Move(draftDir, studentDir);

        var fileName = Path.GetFileName(draftRelativePath ?? "");
        if (string.IsNullOrEmpty(fileName)) return null;

        var relativePath = Path.Combine("uploads", subdir, studentId, fileName).Replace('\\', '/');
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        _compression.QueueForCompression(fullPath, compressionType);
        return relativePath;
    }

    public void DeleteDraftFiles(Guid draftId)
    {
        var did = draftId.ToString("N");
        foreach (var subdir in new[] { UploadConstants.ImagesSubdir, UploadConstants.VideosSubdir })
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
        foreach (var subdir in new[] { UploadConstants.ImagesSubdir, UploadConstants.VideosSubdir })
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
