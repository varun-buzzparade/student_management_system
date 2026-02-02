using StudentManagementSystem.Configuration;

namespace StudentManagementSystem.Services.Student.Upload;

public sealed class VideoCompressor : IVideoCompressor
{
    public async Task CompressAsync(string fullPath, CompressionOptions options, string ffmpegPath, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fullPath);
        var tempPath = Path.Combine(Path.GetDirectoryName(fullPath)!, "temp_" + Guid.NewGuid().ToString("N") + ext);

        var args = $"-i \"{fullPath}\" -c:v libx264 -crf {options.VideoCrf} -c:a copy -y \"{tempPath}\"";
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = System.Diagnostics.Process.Start(psi);
        if (process == null) return;

        var readOut = process.StandardOutput.ReadToEndAsync();
        var readErr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(readOut, readErr);

        if (process.ExitCode == 0 && File.Exists(tempPath))
        {
            File.Move(tempPath, fullPath, overwrite: true);
            return;
        }
        if (File.Exists(tempPath))
            try { File.Delete(tempPath); } catch { }
        throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode} for {fullPath}");
    }
}
