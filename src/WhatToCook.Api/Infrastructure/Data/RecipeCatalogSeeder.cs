using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Domain.Planning;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Features.Recipes.Shared;

namespace WhatToCook.Api.Infrastructure.Data;

public static class RecipeCatalogSeeder
{
    public static async Task SeedIfEnabledAsync(
        AppDbContext dbContext,
        IConfiguration configuration,
        IHostEnvironment environment,
        CancellationToken cancellationToken
    )
    {
        var configured = configuration.GetValue<bool?>("SeedData:Enabled");
        var seedEnabled = configured ?? environment.IsDevelopment();

        if (!seedEnabled)
        {
            return;
        }

        if (await dbContext.Recipes.AnyAsync(cancellationToken))
        {
            return;
        }

        var ingredientByNormalizedName = new Dictionary<string, Ingredient>(StringComparer.Ordinal);
        var tagByNormalizedName = new Dictionary<string, Tag>(StringComparer.Ordinal);
        var seededRecipes = new List<Recipe>();
        var profile = configuration["SeedData:Profile"];
        var seedRecipes = string.Equals(profile, "large", StringComparison.OrdinalIgnoreCase)
            ? BuildLargeSeedRecipes()
            : SeedRecipes;

        foreach (var seedRecipe in seedRecipes)
        {
            var recipe = new Recipe
            {
                Title = seedRecipe.Title,
                NormalizedTitle = TextNormalizer.Normalize(seedRecipe.Title),
                Servings = seedRecipe.Servings,
                Source = seedRecipe.Source,
                MainPhotoPath = seedRecipe.MainPhotoPath,
                IsActive = true,
            };

            for (var index = 0; index < seedRecipe.Ingredients.Count; index++)
            {
                var seedIngredient = seedRecipe.Ingredients[index];
                var normalizedIngredientName = TextNormalizer.Normalize(seedIngredient.Name);
                if (
                    !ingredientByNormalizedName.TryGetValue(
                        normalizedIngredientName,
                        out var ingredient
                    )
                )
                {
                    ingredient = new Ingredient
                    {
                        DisplayName = seedIngredient.Name,
                        NormalizedName = normalizedIngredientName,
                    };
                    ingredientByNormalizedName[normalizedIngredientName] = ingredient;
                }

                recipe.Ingredients.Add(
                    new RecipeIngredient
                    {
                        Ingredient = ingredient,
                        QuantityText = seedIngredient.QuantityText,
                        Unit = seedIngredient.Unit,
                        SortOrder = index,
                    }
                );
            }

            for (var index = 0; index < seedRecipe.Steps.Count; index++)
            {
                recipe.Steps.Add(
                    new RecipeStep { Text = seedRecipe.Steps[index], SortOrder = index }
                );
            }

            foreach (var seedTag in seedRecipe.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalizedTag = TextNormalizer.Normalize(seedTag);
                if (!tagByNormalizedName.TryGetValue(normalizedTag, out var tag))
                {
                    tag = new Tag { DisplayName = seedTag, NormalizedName = normalizedTag };
                    tagByNormalizedName[normalizedTag] = tag;
                }

                recipe.RecipeTags.Add(new RecipeTag { Tag = tag });
            }

            dbContext.Recipes.Add(recipe);
            seededRecipes.Add(recipe);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (string.Equals(profile, "large", StringComparison.OrdinalIgnoreCase))
        {
            await SeedLargeMealPlansAsync(dbContext, seededRecipes, cancellationToken);
        }
    }

    private static readonly IReadOnlyList<SeedRecipe> SeedRecipes = BuildSeedRecipes();

    private static IReadOnlyList<SeedRecipe> BuildLargeSeedRecipes()
    {
        var recipes = SeedRecipes.ToList();
        var dishNames = new[]
        {
            "Miska warzywna",
            "Makaron z warzywami",
            "Zupa sezonowa",
            "Pieczone warzywa",
            "Sałatka obiadowa",
            "Kanapki na ciepło",
            "Ryż z dodatkami",
            "Pasta kanapkowa",
            "Bowl śniadaniowy",
            "Danie jednogarnkowe",
        };
        var ingredientNames = new[]
        {
            "napój owsiany",
            "ciecierzyca",
            "pomidory suszone",
            "fasola biała",
            "brokuł",
            "marchew",
            "tofu naturalne",
            "jogurt grecki",
            "płatki owsiane",
            "kasza bulgur",
            "makaron pełnoziarnisty",
            "papryka czerwona",
            "szpinak baby",
            "pestki dyni",
            "soczewica czerwona",
        };
        var tags = new[]
        {
            "szybkie",
            "obiad",
            "kolacja",
            "wege",
            "lunchbox",
            "rodzinne",
            "fit",
            "sezonowe",
            "bez glutenu",
            "15 min",
        };

        for (var index = recipes.Count; index < 100; index++)
        {
            var primaryIngredient = ingredientNames[index % ingredientNames.Length];
            var secondaryIngredient = ingredientNames[(index + 5) % ingredientNames.Length];
            recipes.Add(
                new SeedRecipe(
                    $"Przepis demonstracyjny {index + 1:000}: {dishNames[index % dishNames.Length]}",
                    2 + (index % 5),
                    "Dane demonstracyjne large",
                    null,
                    [
                        new(primaryIngredient, (100 + (index % 4) * 25).ToString(), "g"),
                        new(secondaryIngredient, "1", "szt"),
                        new("oliwa", "1", "łyżka"),
                    ],
                    ["Przygotuj składniki.", "Połącz i dopraw do smaku.", "Podaj od razu."],
                    [tags[index % tags.Length], tags[(index + 3) % tags.Length]]
                )
            );
        }

        return recipes;
    }

    private static async Task SeedLargeMealPlansAsync(
        AppDbContext dbContext,
        IReadOnlyList<Recipe> recipes,
        CancellationToken cancellationToken
    )
    {
        var firstMonday = new DateOnly(2026, 1, 5);
        for (var planIndex = 0; planIndex < 20; planIndex++)
        {
            var dateFrom = firstMonday.AddDays(planIndex * 7);
            var plan = new MealPlan
            {
                Name = $"Plan demonstracyjny {planIndex + 1:00}",
                DateFrom = dateFrom,
                DateTo = dateFrom.AddDays(6),
            };

            for (var dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var recipe = recipes[(planIndex * 7 + dayOffset) % recipes.Count];
                plan.PlannedRecipes.Add(
                    new PlannedRecipe
                    {
                        RecipeId = recipe.Id,
                        PlannedDate = dateFrom.AddDays(dayOffset),
                        Multiplier = 1m,
                        SortOrder = 0,
                    }
                );
            }

            dbContext.MealPlans.Add(plan);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<SeedRecipe> BuildSeedRecipes()
    {
        return
        [
            new(
                "Grecka sałatka",
                4,
                "https://example.com/grecka",
                null,
                [new("pomidor", "3", "szt"), new("ogórek", "1", "szt"), new("feta", "200", "g")],
                ["Pokrój warzywa.", "Dodaj fetę.", "Wymieszaj i dopraw."],
                ["szybkie", "15 min"]
            ),
            new(
                "Zupa pomidorowa",
                4,
                "Przepis rodzinny",
                null,
                [new("pomidor", "800", "g"), new("makaron", "120", "g"), new("bulion", "1", "l")],
                ["Ugotuj bazę.", "Dodaj makaron.", "Gotuj do miękkości."],
                ["obiad", "klasyk"]
            ),
            new(
                "Kurczak cytrynowy z ryżem",
                4,
                "https://example.com/kurczak-cytrynowy",
                null,
                [
                    new("kurczak", "500", "g"),
                    new("cytryna", "1", "szt"),
                    new("czosnek", "3", "zabki"),
                    new("ryż", "250", "g"),
                    new("papryka", "1", "szt"),
                ],
                ["Podsmaż kurczaka.", "Dodaj warzywa i przyprawy.", "Podaj z ugotowanym ryżem."],
                ["ryż", "wysokobiałkowe", "obiad"]
            ),
            new(
                "Makaron pesto ze szpinakiem",
                3,
                "https://example.com/makaron-pesto",
                null,
                [
                    new("makaron", "300", "g"),
                    new("szpinak", "150", "g"),
                    new("oliwa", "2", "łyżki"),
                    new("parmezan", "60", "g"),
                    new("pietruszka", "1", "pęczek"),
                ],
                [
                    "Ugotuj makaron.",
                    "Przygotuj sos na bazie oliwy i zieleniny.",
                    "Wymieszaj i podaj.",
                ],
                ["makaron", "kolacja", "fit"]
            ),
            new(
                "Curry z ciecierzycy",
                4,
                "https://example.com/curry-ciecierzyca",
                null,
                [
                    new("ciecierzyca", "2", "puszki"),
                    new("mleko kokosowe", "400", "ml"),
                    new("imbir", "2", "cm"),
                    new("cebula", "1", "szt"),
                    new("kolendra", "1", "pęczek"),
                ],
                [
                    "Podsmaż cebulę z imbirem.",
                    "Dodaj ciecierzycę i mleko kokosowe.",
                    "Dopraw i podaj z kolendrą.",
                ],
                ["wege", "pikantne", "obiad"]
            ),
            new(
                "Tacos z tuńczykiem",
                3,
                "https://example.com/tacos-tunczyk",
                null,
                [
                    new("tuńczyk", "2", "puszki"),
                    new("kukurydza", "150", "g"),
                    new("fasola", "150", "g"),
                    new("limonka", "1", "szt"),
                    new("awokado", "1", "szt"),
                ],
                [
                    "Wymieszaj farsz.",
                    "Podgrzej placki i wypełnij farszem.",
                    "Podaj z limonką i awokado.",
                ],
                ["na wynos", "lunchbox", "szybkie"]
            ),
            new(
                "Szakszuka klasyczna",
                2,
                "https://example.com/szakszuka",
                null,
                [
                    new("jajko", "4", "szt"),
                    new("pomidor", "4", "szt"),
                    new("cebula", "1", "szt"),
                    new("papryka", "1", "szt"),
                    new("szczypiorek", "1", "pęczek"),
                ],
                [
                    "Podsmaż cebulę i paprykę.",
                    "Dodaj pomidory i duś sos.",
                    "Wbij jajka i gotuj do ścięcia.",
                ],
                ["śniadanie", "klasyk", "15 min"]
            ),
            new(
                "Risotto grzybowe",
                4,
                "https://example.com/risotto-grzybowe",
                null,
                [
                    new("ryż", "300", "g"),
                    new("grzyby", "250", "g"),
                    new("masło", "40", "g"),
                    new("śmietana", "80", "ml"),
                    new("parmezan", "70", "g"),
                ],
                [
                    "Podsmaż ryż na maśle.",
                    "Stopniowo dolewaj płyn i dodaj grzyby.",
                    "Wykończ śmietaną i parmezanem.",
                ],
                ["comfort food", "sezonowe", "obiad"]
            ),
            new(
                "Blacha pieczonych warzyw",
                4,
                "https://example.com/blacha-warzyw",
                null,
                [
                    new("brokuł", "1", "szt"),
                    new("kalafior", "1", "szt"),
                    new("batat", "2", "szt"),
                    new("burak", "2", "szt"),
                    new("cukinia", "1", "szt"),
                ],
                ["Pokrój warzywa.", "Wymieszaj z oliwą i przyprawami.", "Piecz do miękkości."],
                ["pieczone", "bez glutenu", "sezonowe"]
            ),
            new(
                "Smoothie bowl jabłko-banan",
                2,
                "https://example.com/smoothie-bowl",
                null,
                [
                    new("jabłko", "1", "szt"),
                    new("banan", "2", "szt"),
                    new("jogurt", "250", "g"),
                    new("miód", "1", "łyżka"),
                    new("chia", "2", "łyżki"),
                ],
                ["Zblenduj owoce z jogurtem.", "Przełóż do miski.", "Dodaj chia i miód."],
                ["deser", "śniadanie", "fit"]
            ),
            new(
                "Naleśniki z twarogiem i borówkami",
                4,
                "https://example.com/nalesniki-twarog",
                null,
                [
                    new("mąka", "250", "g"),
                    new("mleko", "500", "ml"),
                    new("twaróg", "300", "g"),
                    new("borówki", "150", "g"),
                    new("orzechy", "50", "g"),
                ],
                [
                    "Usmaż naleśniki.",
                    "Przygotuj farsz twarogowy.",
                    "Podaj z borówkami i orzechami.",
                ],
                ["deser", "rodzinne", "kolacja"]
            ),
            new(
                "Łosoś z kaszą i soczewicą",
                3,
                "https://example.com/losos-kasza",
                null,
                [
                    new("łosoś", "450", "g"),
                    new("kasza", "220", "g"),
                    new("soczewica", "160", "g"),
                    new("rukola", "80", "g"),
                    new("cytryna", "1", "szt"),
                ],
                ["Upiecz łososia.", "Ugotuj kaszę i soczewicę.", "Podaj z rukolą i cytryną."],
                ["bez glutenu", "wysokobiałkowe", "obiad"]
            ),
            new(
                "Krem marchewkowo-ziemniaczany",
                4,
                "https://example.com/krem-marchew",
                null,
                [
                    new("marchew", "5", "szt"),
                    new("ziemniak", "3", "szt"),
                    new("bulion", "1.2", "l"),
                    new("śmietana", "100", "ml"),
                    new("pietruszka", "0.5", "pęczka"),
                ],
                [
                    "Ugotuj warzywa w bulionie.",
                    "Zblenduj na krem.",
                    "Podaj ze śmietaną i pietruszką.",
                ],
                ["zupa", "rodzinne", "obiad"]
            ),
        ];
    }

    private sealed record SeedRecipe(
        string Title,
        int Servings,
        string? Source,
        string? MainPhotoPath,
        IReadOnlyList<SeedIngredient> Ingredients,
        IReadOnlyList<string> Steps,
        IReadOnlyList<string> Tags
    );

    private sealed record SeedIngredient(string Name, string? QuantityText, string? Unit);
}
