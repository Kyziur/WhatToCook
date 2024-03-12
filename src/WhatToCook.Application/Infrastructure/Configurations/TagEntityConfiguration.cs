using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class TagEntityConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        _ = builder.HasKey(x => x.Id);
        _ = builder.Property(x => x.Name);
        _ = builder.HasIndex(x => x.Name).IsUnique();

        _ = builder
    .HasMany(x => x.Recipes)
    .WithMany(x => x.Tags)
    .UsingEntity<RecipeTag>(x => x.HasOne(z => z.Recipe).WithMany(), x => x.HasOne(z => z.Tag).WithMany());
    }
}
