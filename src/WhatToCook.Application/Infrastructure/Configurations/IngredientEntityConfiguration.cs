using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class IngredientEntityConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Name);

        _ = builder.HasOne(x => x.Recipe).WithMany(x => x.Ingredients);
    }
}
