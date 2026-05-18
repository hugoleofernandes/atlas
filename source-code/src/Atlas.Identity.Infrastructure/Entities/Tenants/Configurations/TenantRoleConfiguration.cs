using Atlas.BuildingBlocks.Persistence.Audits;
using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Configurations;

public sealed class TenantRoleConfiguration : IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> b)
    {
        b.ToTable("tenant_roles");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        b.Property(x => x.IsSystem)
            .HasDefaultValue(false);

        b.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();

        // RolePermission stored as a child table
        b.OwnsMany(x => x.Permissions, p =>
        {
            p.ToTable("role_permissions");

            p.WithOwner().HasForeignKey("TenantRoleId");

            p.Property(rp => rp.Code)
                .HasColumnName("code")
                .HasMaxLength(100)
                .IsRequired();

            p.HasKey("TenantRoleId", "Code");
        });

        AuditableEntityConfiguration.Configure(b);
    }
}
