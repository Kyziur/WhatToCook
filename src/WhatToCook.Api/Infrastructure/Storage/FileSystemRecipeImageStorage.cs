using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace WhatToCook.Api.Infrastructure.Storage;

public sealed class FileSystemRecipeImageStorage(
    IConfiguration configuration,
    IWebHostEnvironment environment
) : IRecipeImageStorage
{
    private const int MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxImagePixels = 20_000_000;
    private readonly string rootPath = RecipeImagePathResolver.Resolve(
        configuration,
        environment.ContentRootPath
    );

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxImageBytes)
        {
            throw new InvalidDataException("The image is empty or exceeds the 5 MB limit.");
        }

        Directory.CreateDirectory(rootPath);

        await using var input = file.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var imageBytes = content.GetBuffer().AsSpan(0, checked((int)content.Length));
        var safeExtension = DetectImageExtension(imageBytes);
        if (safeExtension is null)
        {
            throw new InvalidDataException("Only JPEG, PNG, GIF and WebP images are supported.");
        }

        content.Position = 0;
        ImageInfo imageInfo;
        try
        {
            imageInfo = Image.Identify(new DecoderOptions(), content);
        }
        catch (Exception exception)
            when (exception is InvalidImageContentException or UnknownImageFormatException)
        {
            throw new InvalidDataException("The image content is invalid.");
        }
        if (
            imageInfo.Width <= 0
            || imageInfo.Height <= 0
            || (long)imageInfo.Width * imageInfo.Height > MaxImagePixels
        )
        {
            throw new InvalidDataException("The image is invalid or exceeds the pixel limit.");
        }

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var path = Path.Combine(rootPath, fileName);

        await using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None
        );
        content.Position = 0;
        await content.CopyToAsync(stream, cancellationToken);

        return fileName;
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
        {
            return Task.CompletedTask;
        }

        var path = Path.Combine(rootPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private static string? DetectImageExtension(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[..3].SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            return ".jpg";
        if (
            header.Length >= 8
            && header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
        )
            return ".png";
        if (
            header.Length >= 6
            && (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8))
        )
            return ".gif";
        if (
            header.Length >= 12
            && header[..4].SequenceEqual("RIFF"u8)
            && header.Slice(8, 4).SequenceEqual("WEBP"u8)
        )
            return ".webp";
        return null;
    }
}
