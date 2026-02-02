using StudentManagementSystem.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace StudentManagementSystem.Services.Student.Upload;

public sealed class ImageCompressor : IImageCompressor
{
    public async Task CompressAsync(string fullPath, CompressionOptions options, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fullPath);
        var dir = Path.GetDirectoryName(fullPath)!;
        var tempPath = Path.Combine(dir, "temp_" + Guid.NewGuid().ToString("N") + ext);

        try
        {
            await using (var input = File.OpenRead(fullPath))
            using (var image = await Image.LoadAsync(input, cancellationToken))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(options.ImageMaxWidth, options.ImageMaxHeight),
                    Mode = ResizeMode.Max
                }));

                var isJpeg = ext is ".jpeg" or ".jpg";
                if (isJpeg)
                    await image.SaveAsJpegAsync(tempPath, new JpegEncoder { Quality = options.ImageJpegQuality }, cancellationToken);
                else
                    await image.SaveAsPngAsync(tempPath, new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }, cancellationToken);
            }

            File.Move(tempPath, fullPath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
                try { File.Delete(tempPath); } catch { }
            throw;
        }
    }
}
