using Atlas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.IdentityAuditLogs;

public sealed class IdentityUserConfig
    : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> b)
    {
        b.ToTable("identity_users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.ExternalId)
            .HasMaxLength(200);

        b.Property(x => x.IsActive)
            .IsRequired();

        b.HasIndex(x => x.ExternalId);
        b.HasIndex(x => x.IsActive);
    }
}