using IkiForma.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkiForma.Data.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("player");

        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.WikidataId).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => x.WikidataId).IsUnique();
    }
}
