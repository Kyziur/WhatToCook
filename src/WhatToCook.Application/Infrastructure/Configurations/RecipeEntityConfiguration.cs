using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class RecipeEntityConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Name);
        builder.Property(x => x.Image);
        builder.Property(x => x.Description);
        builder.Property(x => x.TimeToPrepare);

        builder.HasMany(x => x.RecipePlanOfMeals).WithOne(rp => rp.Recipe).HasForeignKey(rp => rp.RecipeId);
        builder.HasMany(x => x.Ingredients).WithOne(x => x.Recipe);
        builder
            .HasMany(x => x.Tags)
            .WithMany(x => x.Recipes)
            .UsingEntity<RecipeTag>(
            x => x.HasOne(rt => rt.Tag).WithMany().HasForeignKey(x => x.TagId),
                                x => x.HasOne(rt => rt.Recipe).WithMany().HasForeignKey(x => x.RecipeId));

        builder.OwnsOne(x => x.Statistics);

        builder.Navigation(x => x.Tags).AutoInclude();
    }
}