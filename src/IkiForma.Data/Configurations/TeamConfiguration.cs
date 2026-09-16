using IkiForma.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkiForma.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("team");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.WikidataId).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => x.WikidataId).IsUnique();

        builder.HasOne(x => x.Sport)
            .WithMany(x => x.Teams)
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.League)
            .WithMany(x => x.Teams)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
