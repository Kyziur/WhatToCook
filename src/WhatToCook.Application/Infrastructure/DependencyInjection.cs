using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhatToCook.Application.Infrastructure.Images;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string wwwRootDir) => services.AddDbContext<DatabaseContext>()
            .AddScoped<IRecipesRepository, RecipesRepository>()
            .AddScoped<IMealPlanningRepository, MealPlanningRepository>()
            .AddScoped<IFileSaver, FileSaver>()
            .AddScoped<IImagesManager>(serviceProvider =>
            {
                var fileSaver = serviceProvider.GetRequiredService<IFileSaver>();
                var logger = serviceProvider.GetRequiredService<ILogger<ImagesManager>>();
                return new ImagesManager(wwwRootDir, fileSaver, logger);
            })
            .AddScoped<ITagsRepository, TagsRepository>();
}