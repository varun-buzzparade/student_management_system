namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Resolves the path to ffmpeg.exe. Config path takes precedence; otherwise searches PATH.
/// </summary>
public interface IFfmpegLocator
{
    string? FindFfmpeg();
}
