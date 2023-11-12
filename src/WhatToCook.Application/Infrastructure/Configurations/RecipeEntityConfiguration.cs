using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class RecipeEntityConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name);
        builder.Property(x => x.Image);
        builder.Property(x => x.Description);
        builder.Property(x => x.TimeToPrepare);

        builder.HasMany(x => x.RecipePlanOfMeals).WithOne(rp => rp.Recipe).HasForeignKey(rp => rp.RecipeId);
        builder.HasMany(x => x.Ingredients).WithOne(x => x.Recipe);

        builder.OwnsOne(x => x.Statistics);

        builder.HasIndex(x => x.Name).IsUnique();

    }
}
