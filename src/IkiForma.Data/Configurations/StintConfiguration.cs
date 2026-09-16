using IkiForma.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkiForma.Data.Configurations;

public class StintConfiguration : IEntityTypeConfiguration<Stint>
{
    public void Configure(EntityTypeBuilder<Stint> builder)
    {
        builder.ToTable("stint");

        // Kesişim sorgusu team_id ile eşitlik filtresi yapar (WHERE team_id = @A),
        // bu yüzden team_id lider kolon: index-only scan ile player_id listesi doğrudan okunur.
        builder.HasIndex(x => new { x.TeamId, x.PlayerId });

        builder.HasOne(x => x.Player)
            .WithMany(x => x.Stints)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Team)
            .WithMany(x => x.Stints)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
