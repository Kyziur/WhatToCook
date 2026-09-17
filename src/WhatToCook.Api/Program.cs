using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using WhatToCook.Api.Features.Planning.Plans;
using WhatToCook.Api.Features.Recipes.Details;
using WhatToCook.Api.Features.Recipes.Editor;
using WhatToCook.Api.Features.Recipes.Imports;
using WhatToCook.Api.Features.Recipes.Library;
using WhatToCook.Api.Features.Recipes.Search;
using WhatToCook.Api.Features.Recipes.Shared;
using WhatToCook.Api.Features.Shopping.Lists;
using WhatToCook.Api.Infrastructure.Data;
using WhatToCook.Api.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024
);
builder.Services.AddOpenApi();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddAppDbContext();
builder.Services.AddHealthChecks().AddCheck<AppDbHealthCheck>("database", tags: ["ready"]);
builder
    .Services.AddHttpClient("recipe-import", client => client.Timeout = TimeSpan.FromSeconds(10))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
builder.Services.AddScoped<IRecipeImageStorage, FileSystemRecipeImageStorage>();
builder.Services.AddScoped<IRecipeWriteService, RecipeWriteService>();
builder.Services.AddScoped<IRecipeImportService, RecipeImportService>();
builder.Services.AddScoped<IRecipeImportPageFetcher, HttpRecipeImportPageFetcher>();
builder.Services.AddRecipeImportSources();
builder.Services.AddScoped<ShoppingListService>();

var app = builder.Build();

var recipeImagesPath = RecipeImagePathResolver.Resolve(
    app.Configuration,
    app.Environment.ContentRootPath
);
Directory.CreateDirectory(recipeImagesPath);
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(recipeImagesPath),
        RequestPath = "/recipe-images",
    }
);
app.UseResponseCompression();
app.MapDefaultEndpoints();
app.MapOpenApi();

await AppDbInitializer.InitializeAsync(
    app.Services,
    app.Configuration,
    app.Environment,
    CancellationToken.None
);

app.MapRecipeLibraryEndpoints();
app.MapRecipeDetailsEndpoints();
app.MapRecipeEditorEndpoints();
app.MapRecipeImportEndpoints();
app.MapRecipeSearchEndpoints();
app.MapMealPlanEndpoints();
app.MapShoppingListEndpoints();

app.Run();

public partial class Program;
