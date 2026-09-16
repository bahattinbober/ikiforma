using IkiForma.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkiForma.Data.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("league");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.WikidataId).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => x.WikidataId).IsUnique();

        builder.HasOne(x => x.Sport)
            .WithMany(x => x.Leagues)
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
