using Microsoft.Extensions.Logging;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Extensions;

namespace WhatToCook.Application.Infrastructure.Images;

public static class ImageConstants
{
    public const string DefaultFileName = "default_image.png";
}
public interface IImagesManager
{
    public Task<string> Save(ImageInfo imageInfo);
    public Task<string> Replace(ImageInfo image);
}

internal class ImagesManager(string applicationDirectory, string imagesDirectory, IFileSaver fileSaver, ILogger<ImagesManager> logger) : IImagesManager
{
    private readonly string _imagesPhysicalPath = Path.Combine(applicationDirectory, imagesDirectory);
    private readonly string _imagesDirectory = imagesDirectory;
    private readonly IFileSaver _fileSaver = fileSaver;
    private readonly ILogger<ImagesManager> _logger = logger;

    public async Task<string> Save(ImageInfo imageInfo)
    {
        var directoryToStoreImages = Path.Combine(_imagesPhysicalPath, _imagesDirectory);
        if (imageInfo.Base64Image.IsEmpty())
        {
            return CreatePathToFile(ImageConstants.DefaultFileName);
        }

        try
        {
            string filePath = CreatePathToFile(imageInfo.GetFileName());
            byte[] imageBytes = imageInfo.GetImageBytes();

            //Save to absolute path .../wwwroot/Images/xyz.jpg
            await _fileSaver.SaveAsync(filePath, imageBytes);

            //Relative path /Images/xyz.jpg
            return Path.Combine(_imagesDirectory, imageInfo.GetFileName());
        }
        catch (Exception exception)
        {
            _logger.LogError("An error occurred while saving the image. Error: {message}", exception.Message);
            throw;
        }
    }

    public async Task<string> Replace(ImageInfo image)
    {
        Delete(image);
        return await Save(image);
    }

    private void Delete(ImageInfo image)
    {
        if (image.Base64Image.IsEmpty())
        {
            return;
        }

        try
        {
            string fullPath = CreatePathToFile(image.GetFileName());
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception exception)
        {
            throw new DomainException($"Failed to delete the existing image: {image.GetFileName()}", exception);
        }
    }

    private string CreatePathToFile(string fileName) => Path.Combine(_imagesPhysicalPath, fileName);
}
