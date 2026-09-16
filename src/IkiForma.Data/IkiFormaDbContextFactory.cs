using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IkiForma.Data;

/// <summary>
/// "dotnet ef migrations" komutunun kullandığı design-time factory. Api'yi tam olarak
/// ayağa kaldırmadan (host, DI, config pipeline) sadece migration üretmek için var.
/// Api/Worker çalışma zamanında bağlantı dizesini kendi config'lerinden alacak, burayı kullanmaz.
/// </summary>
public class IkiFormaDbContextFactory : IDesignTimeDbContextFactory<IkiFormaDbContext>
{
    public IkiFormaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("IKIFORMA_DB_CONNECTION")
            ?? "Host=localhost;Port=5433;Database=ikiforma;Username=ikiforma;Password=ikiforma";

        var optionsBuilder = new DbContextOptionsBuilder<IkiFormaDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new IkiFormaDbContext(optionsBuilder.Options);
    }
}
