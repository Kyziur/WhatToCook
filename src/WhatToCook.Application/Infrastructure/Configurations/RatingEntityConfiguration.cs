using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class RatingEntityConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score);

        builder.HasOne(x => x.User);
        builder.HasOne(x => x.Recipe);
    }

}
