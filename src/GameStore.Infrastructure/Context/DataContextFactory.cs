using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GameStore.Infrastructure.Context;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
        var apiProjectPath = Path.Combine(basePath, "GameStore.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetSection("DatabaseSettings:DefaultConnection").Value;

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("The connection string 'DefaultConnection' was not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DataContext(optionsBuilder.Options);
    }
}
