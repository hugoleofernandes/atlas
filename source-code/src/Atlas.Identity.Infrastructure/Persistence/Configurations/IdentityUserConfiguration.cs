using Atlas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations;

public sealed class IdentityUserConfig : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> b)
    {
        b.ToTable("identity_users");

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