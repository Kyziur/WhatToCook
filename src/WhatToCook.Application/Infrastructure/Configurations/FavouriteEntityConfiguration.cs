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
    internal class FavouriteEntityConfiguration : IEntityTypeConfiguration<Favourite>
    {

        public void Configure(EntityTypeBuilder<Favourite> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.User);
            builder.HasOne(x => x.Recipe);
            builder.HasIndex(x => new { x.RecipeId, x.UserId });
        }
    }
}

//Favourite
//Id | UserId | RecipeId
//1 | 1 | 1
//2 | 1 | 1 //throws error
//3 | 1 | 2