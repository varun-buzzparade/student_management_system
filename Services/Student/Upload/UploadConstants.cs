namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Shared constants for upload validation and compression. Single source of truth (DRY).
/// </summary>
public static class UploadConstants
{
    public const int ImageMaxBytes = 5 * 1024 * 1024;   // 5 MB
    public const int VideoMaxBytes = 100 * 1024 * 1024; // 100 MB

    public static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpeg", ".jpg", ".png" };

    public static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".mov", ".mkv", ".avi", ".wmv" };

    public const string ImagesSubdir = "images";
    public const string VideosSubdir = "videos";

    public const string TypeImage = "image";
    public const string TypeVideo = "video";

    public static bool IsImageExtension(string ext) => ImageExtensions.Contains(ext);
    public static bool IsVideoExtension(string ext) => VideoExtensions.Contains(ext);
}
