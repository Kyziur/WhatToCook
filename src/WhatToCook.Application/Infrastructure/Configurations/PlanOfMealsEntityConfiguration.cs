using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class PlanOfMealsEntityConfiguration : IEntityTypeConfiguration<PlanOfMeals>
{
    public void Configure(EntityTypeBuilder<PlanOfMeals> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Name).IsRequired();
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.User);
        builder.HasMany(x => x.Recipes).WithMany(x => x.PlansOfMeals);
        builder.Property(x => x.FromDate);
        builder.Property(x => x.ToDate);
    }
}