using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations
{
    internal class RecipePlanOfMealsEntityConfiguration : IEntityTypeConfiguration<RecipePlanOfMeals>
    {
        public void Configure(EntityTypeBuilder<RecipePlanOfMeals> builder)
        {
            builder.HasKey(rp => new { rp.RecipeId, rp.PlanOfMealsId });
            builder.HasOne(rp => rp.Recipe)
               .WithMany(r => r.RecipePlanOfMeals)
               .HasForeignKey(rp => rp.RecipeId);

            builder.HasOne(rp => rp.PlanOfMeals)
                .WithMany(p => p.RecipePlanOfMeals)
                .HasForeignKey(rp => rp.PlanOfMealsId);

            builder.Property(x => x.Day);
        }
    }
}
