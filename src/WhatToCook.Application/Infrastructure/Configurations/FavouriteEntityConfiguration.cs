using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class FavouriteEntityConfiguration : IEntityTypeConfiguration<Favourite>
{
    public void Configure(EntityTypeBuilder<Favourite> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User);

        builder.HasOne(x => x.Recipe);
    }
}
