using System.Net.Http.Headers;
using IkiForma.Data;
using IkiForma.Worker.Wikidata;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<IkiFormaDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("IkiForma"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddHttpClient<WikidataClient>(client =>
{
    client.BaseAddress = new Uri("https://query.wikidata.org/");
    // Wikimedia politikası: anonim/tanımlanamayan istemciler engellenebilir.
    // https://meta.wikimedia.org/wiki/User-Agent_policy
    client.DefaultRequestHeaders.UserAgent.ParseAdd("IkiForma/0.1 (https://github.com/bahattinbober/ikiforma)");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sparql-results+json"));
    client.Timeout = TimeSpan.FromSeconds(100);
});

builder.Services.AddHostedService<WikidataSyncService>();

var host = builder.Build();
host.Run();
