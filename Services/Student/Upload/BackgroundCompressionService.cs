using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentManagementSystem.Configuration;

namespace StudentManagementSystem.Services.Student.Upload;

/// <summary>
/// Orchestrates async compression: queues files, delegates to IImageCompressor/IVideoCompressor (SRP, DIP).
/// </summary>
public sealed class BackgroundCompressionService : BackgroundService, IBackgroundCompressionService
{
    private readonly Channel<(string FullPath, string Type)> _channel = Channel.CreateUnbounded<(string, string)>();
    private readonly IOptions<CompressionOptions> _options;
    private readonly IImageCompressor _imageCompressor;
    private readonly IVideoCompressor _videoCompressor;
    private readonly IFfmpegLocator _ffmpegLocator;
    private readonly ILogger<BackgroundCompressionService> _logger;

    public BackgroundCompressionService(
        IOptions<CompressionOptions> options,
        IImageCompressor imageCompressor,
        IVideoCompressor videoCompressor,
        IFfmpegLocator ffmpegLocator,
        ILogger<BackgroundCompressionService> logger)
    {
        _options = options;
        _imageCompressor = imageCompressor;
        _videoCompressor = videoCompressor;
        _ffmpegLocator = ffmpegLocator;
        _logger = logger;
    }

    public void QueueForCompression(string fullPath, string type)
    {
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            _logger.LogDebug("QueueForCompression skipped: path empty or file missing: {Path}", fullPath);
            return;
        }
        try
        {
            _channel.Writer.TryWrite((fullPath, type));
            _logger.LogInformation("Queued {Type} for compression: {Path}", type, fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to queue compression for {Path}", fullPath);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background compression service started");
        var opts = _options.Value;

        await foreach (var (fullPath, type) in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var ext = Path.GetExtension(fullPath);
                var isImage = string.Equals(type, UploadConstants.TypeImage, StringComparison.OrdinalIgnoreCase) || UploadConstants.IsImageExtension(ext);
                var isVideo = string.Equals(type, UploadConstants.TypeVideo, StringComparison.OrdinalIgnoreCase) || UploadConstants.IsVideoExtension(ext);

                if (isImage)
                {
                    _logger.LogInformation("Starting image compression: {Path}", fullPath);
                    await _imageCompressor.CompressAsync(fullPath, opts, stoppingToken);
                    _logger.LogInformation("Image compressed successfully: {Path}", fullPath);
                }
                else if (isVideo)
                {
                    _logger.LogInformation("Starting video compression: {Path}", fullPath);
                    var ffmpeg = _ffmpegLocator.FindFfmpeg();
                    if (string.IsNullOrEmpty(ffmpeg))
                    {
                        _logger.LogWarning("FFmpeg not found. Video compression skipped for {Path}", fullPath);
                        continue;
                    }
                    await _videoCompressor.CompressAsync(fullPath, opts, ffmpeg, stoppingToken);
                    _logger.LogInformation("Video compressed successfully: {Path}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compression failed for {Path}", fullPath);
            }
        }
    }
}
