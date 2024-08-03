using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class PlanOfMealsEntityConfiguration : IEntityTypeConfiguration<PlanOfMeals>
{
    public void Configure(EntityTypeBuilder<PlanOfMeals> builder)
    {
        _ = builder.HasIndex(x => x.Name).IsUnique();
        _ = builder.Property(x => x.Name).IsRequired();

        _ = builder.Property(x => x.FromDate);
        _ = builder.Property(x => x.ToDate);

        _ = builder.HasKey(x => x.Id);

        _ = builder.HasOne(x => x.User);

        _ = builder.HasMany(x => x.RecipePlanOfMeals).WithOne(rp => rp.PlanOfMeals).HasForeignKey(rp => rp.PlanOfMealsId);
    }
}