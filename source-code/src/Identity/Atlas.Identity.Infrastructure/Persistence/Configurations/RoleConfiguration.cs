using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Identity.Contracts.EntityTypes;
using Atlas.Identity.Domain.Tenants._Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : AuditableAggregateConfiguration<Role>
{
    protected override Guid EntityTypeId => IdentityModuleEntityTypes.Roles.EntityType.Id;

    protected override void ConfigureAuditable(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles");
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

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();

        b.OwnsMany(x => x.Permissions, p =>
        {
            p.ToTable("role_permissions");

            p.WithOwner().HasForeignKey("RoleId");

            p.Property(rp => rp.PermissionId)
                .HasColumnName("permission_id")
                .IsRequired();

            p.HasKey("RoleId", "PermissionId");
        });

        EntityChangeConfiguration.Configure(b);
    }
}
