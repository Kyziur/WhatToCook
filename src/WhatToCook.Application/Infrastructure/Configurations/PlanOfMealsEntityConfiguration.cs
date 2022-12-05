using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class PlanOfMealsEntityConfiguration : IEntityTypeConfiguration<PlanOfMeals>
{
    public void Configure(EntityTypeBuilder<PlanOfMeals> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.User);
        builder.HasOne(x => x.Recipe);
        builder.Property(x => x.FromDate);
        builder.Property(x => x.ToDate);
    }

}