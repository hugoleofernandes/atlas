using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Identity.Contracts.EntityTypes;
using Atlas.Identity.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : AuditedAggregateConfiguration<Permission>
{
    protected override Guid EntityTypeId => IdentityModuleEntityTypes.Permissions.EntityType.Id;

    protected override void ConfigureEntity(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("permissions");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.ModuleId)
            .IsRequired(false);

        b.Property(x => x.ModuleName)
            .HasMaxLength(100)
            .IsRequired(false);

        b.Property(x => x.Code)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Group)
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.IsManager)
            .IsRequired();

        b.Property(x => x.IsRoot)
            .IsRequired()
            .HasDefaultValue(false);

        b.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        b.HasIndex(x => x.Code)
            .IsUnique();

        // Only one row may have is_root = true - enforced via partial unique index in migration.
        // EF does not support partial unique indexes natively; add it manually in the migration.
    }
}
