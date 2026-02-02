namespace StudentManagementSystem.Configuration;

/// <summary>
/// Options for async image and video compression. Binds to appsettings "Compression" section.
/// </summary>
public class CompressionOptions
{
    public const string SectionName = "Compression";

    public int ImageMaxWidth { get; set; } = 1920;
    public int ImageMaxHeight { get; set; } = 1080;
    public int ImageJpegQuality { get; set; } = 85;
    /// <summary>FFmpeg CRF for video (18-28 typical; higher = smaller file, lower quality).</summary>
    public int VideoCrf { get; set; } = 23;
    /// <summary>Optional full path to ffmpeg.exe. If empty, searches PATH.</summary>
    public string? FfmpegPath { get; set; }
}
