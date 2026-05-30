using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Platform.Domain.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Platform.Infrastructure.Persistence.Configurations;

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> b)
    {
        b.ToTable("modules");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        b.HasIndex(x => x.Name).IsUnique();

        b.Property(x => x.IsActive).IsRequired();

        EntityChangeConfiguration.Configure(b);
    }
}
