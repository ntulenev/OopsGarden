using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Storage;

namespace OopsGarden.Tests;

internal sealed class OopsGardenAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Admins:Users:0:UserName"] = "admin",
                ["Admins:Users:0:Password"] = "secret",
                ["ConnectionStrings:OopsGarden"] = "Server=(localdb)\\mssqllocaldb;Database=OopsGardenTests;Trusted_Connection=True"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<GardenDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<GardenDbContext>>();
            services.AddDbContext<GardenDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
