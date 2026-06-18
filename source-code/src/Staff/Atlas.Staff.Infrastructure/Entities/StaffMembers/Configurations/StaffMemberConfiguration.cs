using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Staff.Contracts.EntityTypes;
using Atlas.Staff.Domain.Entities;
using Atlas.Staff.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Configurations;

public sealed class StaffMemberConfiguration : AuditableAggregateConfiguration<StaffMember>
{
    protected override Guid EntityTypeId => StaffModuleEntityTypes.StaffMembers.EntityType.Id;

    protected override void ConfigureAuditable(EntityTypeBuilder<StaffMember> b)
    {
        b.ToTable("staff_members");

        b.HasKey(x => x.Id);

        // ── legacy columns — kept for CreateStaffMemberFromInvitation outbox flow ──
        b.Property(x => x.UserId)
            .HasColumnName("user_id");

        b.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        b.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        b.Property(x => x.Role)
            .HasColumnName("role")
            .HasMaxLength(50);

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        // ──────────────────────────────────────────────────────────────────────────

        b.Property(x => x.PartyId)
            .HasColumnName("party_id");

        b.Property(x => x.EmployeeNumber)
            .HasColumnName("employee_number")
            .HasMaxLength(20);

        b.Property(x => x.ContractType)
            .HasColumnName("contract_type")
            .HasConversion<string>()
            .HasMaxLength(20);

        b.Property(x => x.HireDate)
            .HasColumnName("hire_date");

        b.Property(x => x.TerminationDate)
            .HasColumnName("termination_date");

        b.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(StaffStatus.Active);

        b.HasIndex(x => new { x.TenantId, x.PartyId })
            .IsUnique()
            .HasFilter("party_id IS NOT NULL");
    }
}
