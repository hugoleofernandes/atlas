using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);

        b.Property(x => x.ExternalId)
            .HasMaxLength(64);

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasIndex(x => x.ExternalId)
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL");
    }
}