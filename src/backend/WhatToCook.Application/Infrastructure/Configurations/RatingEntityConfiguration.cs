using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class RatingEntityConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Score);

        _ = builder.HasOne(x => x.User);
        _ = builder.HasOne(x => x.Recipe);
    }
}
