using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cosmium.Web.Data;

public class WebDbContextFactory : IDesignTimeDbContextFactory<WebDbContext>
{
    public WebDbContext CreateDbContext(string[] args)
    {
        WebEfCoreEntityExtensionMappings.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<WebDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new WebDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}