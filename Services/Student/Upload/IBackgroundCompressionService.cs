namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Queues files for async compression. User gets immediate response; compression runs in background.
/// </summary>
public interface IBackgroundCompressionService
{
    /// <summary>Queue a file for compression. Full path to the file on disk. Type: "image" or "video".</summary>
    void QueueForCompression(string fullPath, string type);
}
