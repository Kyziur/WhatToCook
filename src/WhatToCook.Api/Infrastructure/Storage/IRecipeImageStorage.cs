namespace WhatToCook.Api.Infrastructure.Storage;

public interface IRecipeImageStorage
{
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken);

    Task DeleteAsync(string fileName, CancellationToken cancellationToken);
}
