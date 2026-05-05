using Atlas.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Users;

public sealed class UserConfig
    : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.ExternalId)
            .HasMaxLength(200);

        //b.Property(x => x.IsActive)
        //    .IsRequired();

        b.HasIndex(x => x.ExternalId);
        //b.HasIndex(x => x.IsActive);
    }
}