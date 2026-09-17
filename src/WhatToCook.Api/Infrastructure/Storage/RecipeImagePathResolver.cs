namespace WhatToCook.Api.Infrastructure.Storage;

public static class RecipeImagePathResolver
{
    public static string Resolve(IConfiguration configuration, string contentRootPath)
    {
        var configured = configuration["Storage:RecipeImagesPath"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            return Path.Combine(contentRootPath, "storage", "recipe-images");
        }

        return Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(contentRootPath, configured));
    }
}
