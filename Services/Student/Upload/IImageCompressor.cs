using StudentManagementSystem.Configuration;

namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Compresses an image file in place. Resizes to max dimensions and applies format-specific compression.
/// </summary>
public interface IImageCompressor
{
    Task CompressAsync(string fullPath, CompressionOptions options, CancellationToken cancellationToken = default);
}
