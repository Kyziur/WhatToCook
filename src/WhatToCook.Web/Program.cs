using WhatToCook.Web;
using WhatToCook.Web.Components;
using WhatToCook.Web.Components.Features.Planning.Services;
using WhatToCook.Web.Components.Features.Recipes.Editor.Services;
using WhatToCook.Web.Components.Features.Recipes.Import.Services;
using WhatToCook.Web.Components.Features.Recipes.Shared;
using WhatToCook.Web.Components.Features.Shopping.Services;
using WhatToCook.Web.Components.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddApiHttpClient<IRecipeEditorService, ApiRecipeEditorService>();
builder.Services.AddApiHttpClient<IRecipeImportService, ApiRecipeImportService>();
builder.Services.AddApiHttpClient<IRecipeCatalogService, ApiRecipeCatalogService>();
builder.Services.AddApiHttpClient<IMealPlanService, ApiMealPlanService>();
builder.Services.AddApiHttpClient<IShoppingListService, ApiShoppingListService>();
builder.Services.AddNamedApiHttpClient("api-media");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();
app.UseResponseCompression();
app.MapDefaultEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
if (app.Environment.IsDevelopment())
{
    app.MapGet(
        "/import-fixtures/recipe-import/{kind}",
        (string kind, string? title, string? servings) =>
        {
            var safeTitle = string.IsNullOrWhiteSpace(title) ? "Importowany przepis" : title.Trim();
            var safeServings = string.IsNullOrWhiteSpace(servings) ? "4 porcje" : servings.Trim();

            var html = kind.ToLowerInvariant() switch
            {
                "basic" => BuildImportFixtureHtml(safeTitle, safeServings),
                "missing-servings" => BuildImportFixtureHtml(safeTitle, "keksowka 11 x 30 cm"),
                _ => null,
            };

            return html is null
                ? Results.NotFound()
                : Results.Content(html, "text/html; charset=utf-8");
        }
    );
}
app.MapGet(
    "/recipe-images/{*filePath}",
    async (
        string filePath,
        IHttpClientFactory httpClientFactory,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) =>
    {
        var safePath = filePath.Replace('\\', '/').TrimStart('/');
        if (
            string.IsNullOrWhiteSpace(safePath) || safePath.Contains("..", StringComparison.Ordinal)
        )
        {
            return Results.BadRequest();
        }

        var client = httpClientFactory.CreateClient("api-media");
        var encodedSegments = safePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);
        var encodedPath = string.Join('/', encodedSegments);

        using var response = await client.GetAsync(
            $"/recipe-images/{encodedPath}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return Results.NotFound();
        }

        httpContext.Response.ContentType =
            response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        httpContext.Response.ContentLength = response.Content.Headers.ContentLength;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(httpContext.Response.Body, cancellationToken);
        return Results.Empty;
    }
);

app.Run();

static string BuildImportFixtureHtml(string title, string servings) =>
    $$"""
        <html>
          <body>
            <h1>{{title}}</h1>
            <div>Liczba porcji: {{servings}}</div>
            <section id="ingredients">
              <h2>Skladniki</h2>
              <ul>
                <li>250 g makaron</li>
                <li>3 lyzki pesto</li>
                <li>30 g parmezan</li>
              </ul>
            </section>
            <section id="steps">
              <h2>Przygotowanie</h2>
              <ol>
                <li>Ugotuj makaron.</li>
                <li>Wymieszaj z pesto i parmezanem.</li>
              </ol>
            </section>
          </body>
        </html>
        """;
