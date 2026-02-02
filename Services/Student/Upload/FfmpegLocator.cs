using Microsoft.Extensions.Options;
using StudentManagementSystem.Configuration;

namespace StudentManagementSystem.Services.Student.Upload;

public sealed class FfmpegLocator : IFfmpegLocator
{
    private readonly IOptions<CompressionOptions> _options;

    public FfmpegLocator(IOptions<CompressionOptions> options)
    {
        _options = options;
    }

    public string? FindFfmpeg()
    {
        var configured = _options.Value.FfmpegPath;
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            return configured;

        var exe = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in path.Split(Path.PathSeparator))
        {
            var full = Path.Combine(dir.Trim(), exe);
            if (File.Exists(full)) return full;
        }
        return null;
    }
}
