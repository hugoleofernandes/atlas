using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Platform.Domain.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Platform.Infrastructure.Persistence.Configurations;

public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<EntityType>
{
    public void Configure(EntityTypeBuilder<EntityType> b)
    {
        b.ToTable("entity_types");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.ModuleId).IsRequired();

        b.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Schema)
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => new { x.ModuleId, x.Name }).IsUnique();

        b.HasOne<Module>()
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        EntityChangeConfiguration.Configure(b);
    }
}
