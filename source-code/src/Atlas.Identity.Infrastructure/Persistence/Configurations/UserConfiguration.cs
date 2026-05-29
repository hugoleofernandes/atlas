using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.ExternalId)
            .HasConversion(
                id => id.Value,
                value => ExternalId.Create(value)
            )
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value)
            )
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.RoleId)
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasIndex(x => x.ExternalId);
        b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

        // User is its own aggregate root — FK to Tenant without navigation property
        b.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        EntityChangeConfiguration.Configure(b);
    }
}
