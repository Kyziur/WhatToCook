using Microsoft.EntityFrameworkCore;
using WhatToCook.Api.Domain.Planning;
using WhatToCook.Api.Domain.Recipes;
using WhatToCook.Api.Domain.Shopping;

namespace WhatToCook.Api.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<RecipeImportDraft> RecipeImportDrafts => Set<RecipeImportDraft>();
    public DbSet<RecipeImportIngredient> RecipeImportIngredients => Set<RecipeImportIngredient>();
    public DbSet<RecipeImportStep> RecipeImportSteps => Set<RecipeImportStep>();
    public DbSet<RecipeImportTag> RecipeImportTags => Set<RecipeImportTag>();
    public DbSet<RecipeImportIssue> RecipeImportIssues => Set<RecipeImportIssue>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<PlannedRecipe> PlannedRecipes => Set<PlannedRecipe>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.ToTable("recipes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NormalizedTitle).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(500);
            entity.Property(x => x.MainPhotoPath).HasMaxLength(500);
            entity.HasIndex(x => x.NormalizedTitle).IsUnique();
            entity.HasIndex(x => new { x.IsActive, x.Title });
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.ToTable("ingredients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NormalizedName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.NormalizedName).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.NormalizedName).IsUnique();
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.ToTable("recipe_ingredients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QuantityText).HasMaxLength(64);
            entity.Property(x => x.Unit).HasMaxLength(32);
            entity
                .HasOne(x => x.Recipe)
                .WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.Ingredient)
                .WithMany(x => x.RecipeIngredients)
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.RecipeId, x.SortOrder });
        });

        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.ToTable("recipe_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(4000).IsRequired();
            entity
                .HasOne(x => x.Recipe)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RecipeId, x.SortOrder });
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.ToTable("recipe_tags");
            entity.HasKey(x => new { x.RecipeId, x.TagId });
            entity
                .HasOne(x => x.Recipe)
                .WithMany(x => x.RecipeTags)
                .HasForeignKey(x => x.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.Tag)
                .WithMany(x => x.RecipeTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeImportDraft>(entity =>
        {
            entity.ToTable("recipe_import_drafts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceType).HasConversion<int>();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.SourceUrl).HasMaxLength(2000);
            entity.Property(x => x.Source).HasMaxLength(2000);
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.SourceType);
        });

        modelBuilder.Entity<RecipeImportIngredient>(entity =>
        {
            entity.ToTable("recipe_import_ingredients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.QuantityText).HasMaxLength(64);
            entity.Property(x => x.Unit).HasMaxLength(32);
            entity
                .HasOne(x => x.Draft)
                .WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.DraftId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.DraftId, x.SortOrder });
        });

        modelBuilder.Entity<RecipeImportStep>(entity =>
        {
            entity.ToTable("recipe_import_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(4000).IsRequired();
            entity
                .HasOne(x => x.Draft)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.DraftId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.DraftId, x.SortOrder });
        });

        modelBuilder.Entity<RecipeImportTag>(entity =>
        {
            entity.ToTable("recipe_import_tags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(100).IsRequired();
            entity
                .HasOne(x => x.Draft)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.DraftId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.DraftId, x.SortOrder });
        });

        modelBuilder.Entity<RecipeImportIssue>(entity =>
        {
            entity.ToTable("recipe_import_issues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FieldPath).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Severity).HasConversion<int>();
            entity
                .HasOne(x => x.Draft)
                .WithMany(x => x.Issues)
                .HasForeignKey(x => x.DraftId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.DraftId, x.SortOrder });
        });

        modelBuilder.Entity<MealPlan>(entity =>
        {
            entity.ToTable("meal_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.DateFrom, x.DateTo });
            entity
                .HasOne(x => x.ShoppingList)
                .WithOne(x => x.MealPlan)
                .HasForeignKey<ShoppingList>(x => x.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlannedRecipe>(entity =>
        {
            entity.ToTable("planned_recipes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Multiplier).HasPrecision(10, 2);
            entity
                .HasOne(x => x.MealPlan)
                .WithMany(x => x.PlannedRecipes)
                .HasForeignKey(x => x.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(x => x.Recipe)
                .WithMany()
                .HasForeignKey(x => x.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new
            {
                x.MealPlanId,
                x.PlannedDate,
                x.SortOrder,
            });
            entity
                .HasIndex(x => new
                {
                    x.MealPlanId,
                    x.PlannedDate,
                    x.RecipeId,
                })
                .IsUnique();
        });

        modelBuilder.Entity<ShoppingList>(entity =>
        {
            entity.ToTable("shopping_lists");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MealPlanId).IsUnique();
            entity.HasIndex(x => x.UpdatedAt);
        });

        modelBuilder.Entity<ShoppingListItem>(entity =>
        {
            entity.ToTable("shopping_list_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NormalizedName).HasMaxLength(200);
            entity.Property(x => x.QuantityText).HasMaxLength(64);
            entity.Property(x => x.Unit).HasMaxLength(32);
            entity.Property(x => x.State).HasConversion<int>();
            entity
                .HasOne(x => x.ShoppingList)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ShoppingListId, x.SortOrder });
            entity.HasIndex(x => new
            {
                x.ShoppingListId,
                x.IsManual,
                x.NormalizedName,
                x.Unit,
            });
        });
    }
}
