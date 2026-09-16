using IkiForma.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkiForma.Data;

public class IkiFormaDbContext(DbContextOptions<IkiFormaDbContext> options) : DbContext(options)
{
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Stint> Stints => Set<Stint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IkiFormaDbContext).Assembly);
    }
}
