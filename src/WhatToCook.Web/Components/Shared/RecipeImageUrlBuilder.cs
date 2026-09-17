namespace WhatToCook.Web.Components.Shared;

public static class RecipeImageUrlBuilder
{
    public static string Build(string? mainPhotoPath)
    {
        if (string.IsNullOrWhiteSpace(mainPhotoPath))
        {
            return string.Empty;
        }

        var normalizedPath = mainPhotoPath.Replace('\\', '/').TrimStart('/');
        var encodedPath = string.Join(
            '/',
            normalizedPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString)
        );

        return $"/recipe-images/{encodedPath}";
    }
}
