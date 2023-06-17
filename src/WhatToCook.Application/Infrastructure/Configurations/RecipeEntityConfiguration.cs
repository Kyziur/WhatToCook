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
        builder.HasMany(x => x.Ingredients).WithOne(x => x.Recipe);
        builder.HasIndex(x => x.Name);
        builder.Property(x => x.Description);
        builder.Property(x => x.TimeToPrepare);
        builder.OwnsOne(x => x.Statistics);
        builder.Property(x => x.Image);
        builder.HasMany(x => x.PlansOfMeals).WithMany(x => x.Recipes);
    }
}
