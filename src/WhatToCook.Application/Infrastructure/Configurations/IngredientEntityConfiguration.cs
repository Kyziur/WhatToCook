using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class IngredientEntityConfiguration : IEntityTypeConfiguration<Ingredient>
{


    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name);
        builder.HasOne(x => x.Recipe).WithMany(x => x.Ingredients);
    }
}
