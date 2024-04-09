using Microsoft.Extensions.Logging;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Extensions;

namespace WhatToCook.Application.Infrastructure.Images;

public interface IImagesManager
{
    public Task<string> Save(ImageInfo imageInfo);
    public Task<string> Replace(ImageInfo image);
}

internal class ImagesManager(string imagesDirectory, IFileSaver fileSaver, ILogger<ImagesManager> logger) : IImagesManager
{
    private readonly string _imagesDirectory = imagesDirectory;
    private readonly IFileSaver _fileSaver = fileSaver;
    private readonly ILogger<ImagesManager> _logger = logger;

    public async Task<string> Save(ImageInfo imageInfo)
    {
        if (imageInfo.Base64Image.IsEmpty())
        {
            return CreatePathToFile(ImageConstants.FileName);
        }

        try
        {
            string filePath = CreatePathToFile(imageInfo.GetFileName());
            byte[] imageBytes = imageInfo.GetImageBytes();

            await _fileSaver.SaveAsync(filePath, imageBytes);

            return filePath;
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

    private string CreatePathToFile(string fileName) => Path.Combine(_imagesDirectory, fileName);
}
