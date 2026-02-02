using StudentManagementSystem.Configuration;

namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Compresses a video file in place using FFmpeg (libx264, CRF, -c:a copy).
/// </summary>
public interface IVideoCompressor
{
    Task CompressAsync(string fullPath, CompressionOptions options, string ffmpegPath, CancellationToken cancellationToken = default);
}
