using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class FavouriteEntityConfiguration : IEntityTypeConfiguration<Favourite>
{
    public void Configure(EntityTypeBuilder<Favourite> builder)
    {
        _ = builder.HasKey(x => x.Id);

        _ = builder.HasOne(x => x.User);

        _ = builder.HasOne(x => x.Recipe);
    }
}
