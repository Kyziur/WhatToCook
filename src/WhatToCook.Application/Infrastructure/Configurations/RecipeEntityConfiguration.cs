using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class RecipeEntityConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        _ = builder.HasKey(x => x.Id);
        _ = builder.HasIndex(x => x.Name).IsUnique();
        _ = builder.Property(x => x.Name);
        _ = builder.Property(x => x.PreparationDescription);
        _ = builder.Property(x => x.TimeToPrepare);
        _ = builder.HasMany(x => x.RecipePlanOfMeals).WithOne(rp => rp.Recipe).HasForeignKey(rp => rp.RecipeId);
        _ = builder.HasMany(x => x.Ingredients).WithOne(x => x.Recipe);
        _ = builder
            .HasMany(x => x.Tags)
            .WithMany(x => x.Recipes)
            .UsingEntity<RecipeTag>(
            x => x.HasOne(rt => rt.Tag).WithMany().HasForeignKey(x => x.TagId),
                                x => x.HasOne(rt => rt.Recipe).WithMany().HasForeignKey(x => x.RecipeId));

        _ = builder.OwnsOne(x => x.Image);
        _ = builder.OwnsOne(x => x.Statistics);

        _ = builder.Navigation(x => x.Tags).AutoInclude();
    }
}