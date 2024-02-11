using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Configurations;

internal class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        _ = builder.HasKey(x => x.Id);
        _ = builder.Property(x => x.Email);
    }
}
