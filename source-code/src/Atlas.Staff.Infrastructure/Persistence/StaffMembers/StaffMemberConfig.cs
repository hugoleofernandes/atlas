using Atlas.Staff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Staff.Infrastructure.Persistence.StaffMembers;

public sealed class StaffMemberConfig : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> b)
    {
        b.ToTable("staff_members");

        b.HasKey(x => x.Id);

        b.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        b.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        b.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique();
    }
}