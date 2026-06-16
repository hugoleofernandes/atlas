using Atlas.Party.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Party.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.Property(x => x.LegalName).HasColumnName("legal_name").HasMaxLength(200).IsRequired();

        b.Property(x => x.TradeName).HasColumnName("trade_name").HasMaxLength(200);

        b.Property(x => x.LegalType).HasConversion<string>().HasColumnName("legal_type").HasMaxLength(30).IsRequired();
    }
}
